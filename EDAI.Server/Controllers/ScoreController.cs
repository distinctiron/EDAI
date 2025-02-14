using DocumentFormat.OpenXml.Office2010.Excel;
using EDAI.Server.Data;
using EDAI.Server.Prompts;
using EDAI.Services;
using EDAI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;
using EDAI.Shared.Tools;
using MudBlazor;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ScoreController(EdaiContext context, IWordFileHandler wordFileHandler, IOpenAiService openAiService) : ControllerBase
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
    public async Task<IResult> GenerateScores(IEnumerable<int> documentIds)
    {
        var documents = context.Documents.Where(d => documentIds.Contains(d.EdaiDocumentId));
        
        var essays = context.Essays.Where(e => documentIds.Contains(e.EdaiDocumentId));

        var assignmentIds = essays.Select(e => e.AssignmentId).Distinct();

        var assignments = context.Assignments.Where(a => assignmentIds.Contains(a.AssignmentId));

        foreach (var assignment in assignments)
        {
            string referenceText = null;

            if (assignment?.ReferenceDocument.DocumentFileExtension == "pdf")
            {
                referenceText = await PdfFileHandler.ExtractTextFromPdf(assignment.ReferenceDocument.DocumentFile);
            }

            var scores = new List<Score>();

            var indexedContents = new List<IEnumerable<IndexedContent>>();
        
            foreach (var document in documents)
            {
                var essayId = essays.Where(e => e.EdaiDocumentId == document.EdaiDocumentId).Select(e => e.EssayId).Single(); 
            
                using (MemoryStream documentStream = new MemoryStream(document.DocumentFile))
                {
                    using MemoryStream reviewedDocumentStream = new MemoryStream();
                    await documentStream.CopyToAsync(reviewedDocumentStream);
                
                    var indexedContent = await wordFileHandler.GetIndexedContent(documentStream);
                    indexedContent.ToList().ForEach(i => i.EssayId = essayId);
                    context.IndexedContents.AddRange(indexedContent);
                    indexedContents.Add(indexedContent);
                
                    //Consider what order things are written to database to ensure IDs exist
                    var essayFeedback = await OpenAIConversor(indexedContent, assignment.Description, referenceText);

                    var comments = essayFeedback.Comments;
                    var score = essayFeedback.EssayScore;
                    score.EssayId = essayId;
                
                    context.FeedbackComments.AddRange(comments);

                    wordFileHandler.AddComments(reviewedDocumentStream, comments);
                    wordFileHandler.AddFeedback(reviewedDocumentStream, score.OverallStructure);
                    wordFileHandler.AddFeedback(reviewedDocumentStream, score.AssignmentAnswer);

                    var reviewedDocument = new EdaiDocument
                    {
                        DocumentName = document.DocumentName + "Reviewed",
                        DocumentFileExtension = document.DocumentFileExtension,
                        DocumentFile = reviewedDocumentStream.ToArray(),
                        UploadDate = DateTime.Now
                    };
                    score.EvaluatedEssayDocument = reviewedDocument;
                
                    scores.Add(score);
                }
            }
            
            context.Scores.AddRange(scores);
            
        }
        
        context.SaveChanges();
        
        return Results.Ok();
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
        return Results.File(bytes, "application/octet-stream", document.DocumentName);
    }

    public async Task<ConversationDTO> OpenAIConversor(IEnumerable<IndexedContent> contents, string asssignmentDescription, string referenceText)
    {
        openAiService.SetIndexedContents(contents, asssignmentDescription, referenceText);
        
        var grammarComments =
            await openAiService.GetCommentsAsync(CommentType.Grammar, TextEvaluatingPrompts.ProvideGrammarComments.Prompt);
        var grammarScore = await openAiService.GetScoreAsync(TextEvaluatingPrompts.ScoreGrammar.Prompt);
        var grammarRecommendation = await openAiService.GetFeedbackAsync(TextEvaluatingPrompts.GiveGrammarRecommendation.Prompt);
        
        var eloquenceComments =
            await openAiService.GetCommentsAsync(CommentType.Eloquence, TextEvaluatingPrompts.ProvideEloquenceComments.Prompt);
        var eloquenceScore = await openAiService.GetScoreAsync(TextEvaluatingPrompts.ScoreEloquence.Prompt);
        var eloquenceRecommendation = await openAiService.GetFeedbackAsync(TextEvaluatingPrompts.GiveEloquenceRecommendation.Prompt);
        
        var argumentComments =
            await openAiService.GetCommentsAsync(CommentType.Logic, TextEvaluatingPrompts.ProvideArgumentComments.Prompt);
        var argumentationScore = await openAiService.GetScoreAsync(TextEvaluatingPrompts.ScoreArgumentation.Prompt);
        var argumentationRecommendation = await openAiService.GetFeedbackAsync(TextEvaluatingPrompts.GiveArgumentationRecommendation.Prompt);

        var assignmenAnswerFeedback = await openAiService.GetFeedbackAsync(TextEvaluatingPrompts.ProvideAssignmentAnswerFeedback.Prompt);
        var structureFeedback = await openAiService.GetFeedbackAsync(TextEvaluatingPrompts.ProvideStructureFeedback.Prompt);

        var assignmentAnswerScore =
            await openAiService.GetScoreAsync(TextEvaluatingPrompts.ScoreAssignmentAnswer.Prompt);
        var structureScore = await openAiService.GetScoreAsync(TextEvaluatingPrompts.ScoreEssayStructure.Prompt);

        var assignmenAnswerRecommendation =
            await openAiService.GetFeedbackAsync(TextEvaluatingPrompts.GiveAssignmentAnswerRecommendation.Prompt);
        var structureReccomendation = await openAiService.GetFeedbackAsync(TextEvaluatingPrompts.GiveEssayStructureRecommendation.Prompt);

        var score = new Score
        {
            GrammarScore = grammarScore,
            GrammarRecommendation = grammarRecommendation,
            EloquenceScore = eloquenceScore,
            EloquenceRecommendation = eloquenceRecommendation,
            ArgumentationScore = argumentationScore,
            ArgumentationRecommendation = argumentationRecommendation,
            AssignmentAnswer = assignmenAnswerFeedback,
            AssignmentAnswerRecommendation = assignmenAnswerRecommendation,
            OverallStructure = structureFeedback,
            OverallStructureRecommendation = structureReccomendation
        };
        
        var comments = grammarComments.Concat(eloquenceComments).Concat(argumentComments);

        var returnDto = new ConversationDTO
        {
            EssayScore = score,
            Comments = comments
        };

        return returnDto;

    }
}