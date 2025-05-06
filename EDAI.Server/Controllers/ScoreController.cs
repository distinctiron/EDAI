using AutoMapper;
using EDAI.Server.Data;
using EDAI.Server.Jobs;
using EDAI.Services.Interfaces;
using EDAI.Shared.Factories;
using EDAI.Shared.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;
using EDAI.Shared.Tools;
using Hangfire;
using CommentsDTO = EDAI.Shared.Models.DTO.OpenAI.CommentsDTO;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ScoreController(EdaiContext context, IWordFileHandlerFactory wordFileHandlerFactory, IOpenAiService openAiService, IMapper mapper) : ControllerBase
{
    [HttpGet(Name = "GetScores")]
    public IEnumerable<Score> GetScores()
    {
        return context.Scores;
    }

    [HttpGet("{id:int}", Name = "GetScoresById")]
    public IResult GetById(int id)
    {
        var score = context.Scores.Find(id);
        return score == null ? Results.NotFound() : Results.Ok(score);
    }

    [HttpPost(Name = "AddScore")]
    public IResult AddScore(Score score)
    {
        context.Scores.Add(score);
        context.SaveChanges();
        return Results.Ok(score.ScoreId);
    }

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

    [HttpDelete("{id:int}", Name = "DeleteScore")]
    public IResult DeleteScore(int id)
    {
        var score = context.Scores.Find(id);
        if (score == null) return Results.NotFound();
        context.Scores.Remove(score);
        context.SaveChanges();
        return Results.Ok(score);
    }

    [HttpPut(Name = "UpdateScore")]
    public IResult UpdateScore(Score score)
    {
        context.Scores.Update(score);
        context.SaveChanges();
        return Results.Ok(score);
    }
    
    [HttpPost("{id:int}/uploadScoredDocumentFile", Name = "UploadScoredDocumentFile")]
    public IResult UploadFile([FromRoute]int id, IFormFile file)
    {
        var score = context.Scores.Find(id);
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
        
        context.SaveChanges();

        return Results.Ok(score);
    }

    [HttpGet("{id:int}/downloadScoredDocumentFile", Name = "DownloadScoredDocumentFile")]
    public IResult DownloadFile(int id)
    {
        var score = context.Scores.Find(id);
        if (score == null) return Results.NotFound();
        var document = context.Documents.Find(score.EvaluatedEssayDocumentId);
        if (document == null) return Results.NotFound();
        var bytes = document.DocumentFile!;
        return Results.File(bytes, "application/octet-stream", document.DocumentName + document.DocumentFileExtension);
    }
}