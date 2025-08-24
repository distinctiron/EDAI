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
using System.Security.Claims;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreController(EdaiContext context, IWordFileHandlerFactory wordFileHandlerFactory, IOpenAiService openAiService, IMapper mapper) : ControllerBase
{
    private async Task<int?> GetUserOrganisationId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Organisation.OrganisationId)
            .SingleOrDefaultAsync();
    }
    [Authorize]
    [HttpGet(Name = "GetScores")]
    public async Task<IEnumerable<Score>> GetScores()
    {
        var orgId = await GetUserOrganisationId();
        return await context.Scores
            .Include(s => s.Essay)!.ThenInclude(e => e.Student)!.ThenInclude(st => st.StudentClass).ThenInclude(sc => sc.Organisation)
            .Where(s => s.Essay!.Student!.StudentClass.Organisation.OrganisationId == orgId)
            .ToListAsync();
    }

    [Authorize]
    [HttpGet("{id:int}", Name = "GetScoresById")]
    public async Task<IResult> GetById(int id)
    {
        var orgId = await GetUserOrganisationId();
        var score = await context.Scores
            .Include(s => s.Essay)!.ThenInclude(e => e.Student)!.ThenInclude(st => st.StudentClass).ThenInclude(sc => sc.Organisation)
            .SingleOrDefaultAsync(s => s.ScoreId == id && s.Essay!.Student!.StudentClass.Organisation.OrganisationId == orgId);
        return score == null ? Results.NotFound() : Results.Ok(score);
    }

    [Authorize]
    [HttpPost(Name = "AddScore")]
    public async Task<IResult> AddScore(Score score)
    {
        var orgId = await GetUserOrganisationId();
        var essay = await context.Essays
            .Include(e => e.Student)!.ThenInclude(s => s.StudentClass).ThenInclude(sc => sc.Organisation)
            .SingleOrDefaultAsync(e => e.EssayId == score.EssayId && e.Student!.StudentClass.Organisation.OrganisationId == orgId);
        if (essay == null) return Results.Forbid();
        context.Scores.Add(score);
        await context.SaveChangesAsync();
        return Results.Ok(score.ScoreId);
    }

    [Authorize]
    [HttpPost("generatescores", Name = "GenerateScores")]
    public async Task<IResult> GenerateScores(GenerateScoreRequestDTO generateScoreRequestDto)
    {
        var orgId = await GetUserOrganisationId();
        var allowedDocs = await context.Essays
            .Include(e => e.Student)!.ThenInclude(s => s.StudentClass).ThenInclude(sc => sc.Organisation)
            .Where(e => e.Student!.StudentClass.Organisation.OrganisationId == orgId)
            .Select(e => e.EdaiDocumentId)
            .ToListAsync();
        if (generateScoreRequestDto.DocumentIds.Except(allowedDocs).Any()) return Results.Forbid();

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
        var orgId = await GetUserOrganisationId();
        var score = await context.Scores
            .Include(s => s.Essay)!.ThenInclude(e => e.Student)!.ThenInclude(st => st.StudentClass).ThenInclude(sc => sc.Organisation)
            .SingleOrDefaultAsync(s => s.ScoreId == id && s.Essay!.Student!.StudentClass.Organisation.OrganisationId == orgId);
        if (score == null) return Results.NotFound();
        context.Scores.Remove(score);
        context.SaveChangesAsync();
        return Results.Ok(score);
    }

    [Authorize]
    [HttpPut(Name = "UpdateScore")]
    public async Task<IResult> UpdateScore(Score score)
    {
        var orgId = await GetUserOrganisationId();
        var exists = await context.Scores
            .Include(s => s.Essay)!.ThenInclude(e => e.Student)!.ThenInclude(st => st.StudentClass).ThenInclude(sc => sc.Organisation)
            .AnyAsync(s => s.ScoreId == score.ScoreId && s.Essay!.Student!.StudentClass.Organisation.OrganisationId == orgId);
        if (!exists) return Results.NotFound();
        context.Scores.Update(score);
        await context.SaveChangesAsync();
        return Results.Ok(score);
    }
    
    [Authorize]
    [HttpPost("{id:int}/uploadScoredDocumentFile", Name = "UploadScoredDocumentFile")]
    public async Task<IResult> UploadFile([FromRoute]int id, IFormFile file)
    {
        var orgId = await GetUserOrganisationId();
        var score = await context.Scores
            .Include(s => s.Essay)!.ThenInclude(e => e.Student)!.ThenInclude(st => st.StudentClass).ThenInclude(sc => sc.Organisation)
            .SingleOrDefaultAsync(s => s.ScoreId == id && s.Essay!.Student!.StudentClass.Organisation.OrganisationId == orgId);
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
        var orgId = await GetUserOrganisationId();
        var score = await context.Scores
            .Include(s => s.Essay)!.ThenInclude(e => e.Student)!.ThenInclude(st => st.StudentClass).ThenInclude(sc => sc.Organisation)
            .Where(s => s.EssayId == id && s.Essay!.Student!.StudentClass.Organisation.OrganisationId == orgId)
            .OrderByDescending(s => s.ScoreId)
            .FirstOrDefaultAsync();
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
        var orgId = await GetUserOrganisationId();
        var scores = await context.Scores
            .Include(s => s.Essay)!.ThenInclude(e => e.Student)!.ThenInclude(st => st.StudentClass).ThenInclude(sc => sc.Organisation)
            .Where(s => ids.Contains(s.EssayId) && s.Essay!.Student!.StudentClass.Organisation.OrganisationId == orgId)
            .GroupBy(s => s.EssayId)
            .Select(g => g.OrderByDescending(s => s.ScoreId).First())
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