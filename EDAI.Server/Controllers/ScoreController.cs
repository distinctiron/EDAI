using System.IO.Compression;
using AutoMapper;
using EDAI.Server.Data;
using EDAI.Server.Jobs;
using EDAI.Services.Interfaces;
using EDAI.Shared.Factories;
using EDAI.Shared.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.Entities;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreController(EdaiContext context, IWordFileHandlerFactory wordFileHandlerFactory, IOpenAiService openAiService, IMapper mapper) : ControllerBase
{
    [Authorize]
    [HttpGet(Name = "GetScores")]
    public async Task<IEnumerable<ScoreDTO>> GetScores()
    {
        var score = await context.Scores.ToListAsync();

        return mapper.Map<IEnumerable<ScoreDTO>>(score);
    }

    [Authorize]
    [HttpGet("{id:int}", Name = "GetScoresById")]
    public async Task<IResult> GetById(int id)
    {
        var score = await context.Scores.FindAsync(id);
        return score == null ? Results.NotFound() : Results.Ok(score);
    }

    [Authorize]
    [HttpPost(Name = "AddScore")]
    public async Task<IResult> AddScore(Score score)
    {
        context.Scores.Add(score);
        await context.SaveChangesAsync();
        return Results.Ok(score.ScoreId);
    }

    [Authorize]
    [HttpPost("generatescores", Name = "GenerateScores")]
    public async Task<IResult> GenerateScores(GenerateScoreRequestDTO generateScoreRequestDto)
    {

        var jobId = BackgroundJob.Enqueue<IGenerateScoreService>(s => s.GenerateScore(generateScoreRequestDto.DocumentIds, generateScoreRequestDto.ConnectionId));
        
        var response = new
        {
            Message = "Documents are being reviewed and scored",
            JobId = jobId
        };
        
        return Results.Accepted(null, response);
    }

    [Authorize]
    [HttpDelete("{id:int}", Name = "DeleteScore")]
    public async Task<IResult> DeleteScore(int id)
    {
        var score = await context.Scores.FindAsync(id);
        if (score == null) return Results.NotFound();
        context.Scores.Remove(score);
        context.SaveChangesAsync();
        return Results.Ok(score);
    }

    [Authorize]
    [HttpPut(Name = "UpdateScore")]
    public async Task<IResult> UpdateScore(Score score)
    {
        context.Scores.Update(score);
        await context.SaveChangesAsync();
        return Results.Ok(score);
    }
    
    [Authorize]
    [HttpPost("{id:int}/uploadScoredDocumentFile", Name = "UploadScoredDocumentFile")]
    public async Task<IResult> UploadFile([FromRoute]int id, IFormFile file)
    {
        var score = await context.Scores.FindAsync(id);
        if (score == null) return Results.NotFound();
        
        MemoryStream memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);

        EdaiDocument edaiDocument = new EdaiDocument
        {
            DocumentName = file.FileName, DocumentFileExtension = file.FileName.Split(".").Last(),
            UploadDate = DateTime.Now
        };
        edaiDocument.DocumentFile = memoryStream.ToArray();

        context.Documents.Add(edaiDocument);
        
        score.EvaluatedEssayDocument = edaiDocument;
        context.Scores.Update(score);
        
        await context.SaveChangesAsync();

        return Results.Ok(score);
    }

    [Authorize]
    [HttpGet("{id:int}/downloadScoredDocumentFile", Name = "DownloadScoredDocumentFile")]
    public async Task<IResult> DownloadFile(int id)
    {
        var score = await context.Scores.Where(s => s.EssayId == id)
            .OrderByDescending(s => s.ScoreId)
            .FirstAsync();
        if (score == null) return Results.NotFound();
        var document = await context.Documents.FindAsync(score.EvaluatedEssayDocumentId);
        if (document == null) return Results.NotFound();
        var bytes = document.DocumentFile!;
        return Results.File(bytes, "application/octet-stream", document.DocumentName + document.DocumentFileExtension);
    }

    [Authorize]
    [HttpGet("bulkdownload", Name = "DownloadMultipleScoredDocumentFiles")]
    public async Task<IResult> DownloadMultipleFiles([FromQuery] List<int> ids)
    {
        var latestScoreIds = await context.Scores
            .Where(s => ids.Contains(s.EssayId))
            .GroupBy(s => s.EssayId)
            .Select(g => g.Max(s => s.ScoreId))
            .ToListAsync();

        var scores = await context.Scores
            .Where(s => latestScoreIds.Contains(s.ScoreId))
            .Select(s => s.EvaluatedEssayDocumentId)
            .ToListAsync();
        
        var documents = await context.Documents.Where(d => scores.Contains(d.EdaiDocumentId)).ToListAsync();

        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen:true))
        {
            foreach (var document in documents)
            {
                var entry = archive.CreateEntry(document.DocumentName + document.DocumentFileExtension,
                    CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                entryStream.Write(document.DocumentFile, 0, document.DocumentFile.Length);
                
            }
        }

        zipStream.Position = 0;
        
        return Results.File(zipStream.ToArray(),"application/zip", "reviewedDocuments.zip");

    }
}