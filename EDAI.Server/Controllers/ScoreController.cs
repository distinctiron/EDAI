using AutoMapper;
using EDAI.Server.Data;
using EDAI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;
using EDAI.Shared.Tools;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ScoreController(EdaiContext context, IWordFileHandler wordFileHandler, IOpenAiService openAiService, IMapper mapper) : ControllerBase
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

            if (assignment?.ReferenceDocument?.DocumentFileExtension == "pdf")
            {
                referenceText = await PdfFileHandler.ExtractTextFromPdf(assignment.ReferenceDocument.DocumentFile);
            }

            var scores = new List<Score>();

            //var indexedContents = new List<IEnumerable<IndexedContent>>();
        
            foreach (var document in documents)
            {
                var essayId = essays.Where(e => e.EdaiDocumentId == document.EdaiDocumentId).Select(e => e.EssayId).Single(); 
            
                using (MemoryStream documentStream = new MemoryStream(document.DocumentFile))
                {
                    using MemoryStream reviewedDocumentStream = new MemoryStream();
                    await documentStream.CopyToAsync(reviewedDocumentStream);
                
                    var indexedContent = await wordFileHandler.GetIndexedContent(documentStream, essayId);
                    context.IndexedContents.AddRange(indexedContent);
                    context.SaveChanges();
                    //indexedContents.Add(indexedContent);
                
                    //Consider what order things are written to database to ensure IDs exist
                    var essayFeedback = await OpenAIConversor(indexedContent, assignment.Description, referenceText);

                    var comments = essayFeedback.Comments;
                    AssignRelatedText(comments, indexedContent, essayId);
                    
                    var score = essayFeedback.EssayScore;
                    score.EssayId = essayId;
                
                    context.FeedbackComments.AddRange(comments);
                    context.SaveChanges();

                    wordFileHandler.AddComments(reviewedDocumentStream, comments);
                    wordFileHandler.AddFeedback(reviewedDocumentStream, score.OverallStructure);
                    wordFileHandler.AddFeedback(reviewedDocumentStream, score.AssignmentAnswer);

                    var fileExtensionIndex = document.DocumentName.IndexOf('.');

                    var reviewedDocument = new EdaiDocument
                    {
                        DocumentName = document.DocumentName.Insert(fileExtensionIndex,"Reviewed") ,
                        DocumentFileExtension = document.DocumentFileExtension,
                        DocumentFile = reviewedDocumentStream.ToArray(),
                        UploadDate = DateTime.Now
                    };
                    context.Documents.Add(reviewedDocument);
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

    private async Task<ConversationDTO> OpenAIConversor(IEnumerable<IndexedContent> contents, string asssignmentDescription, string referenceText)
    {
        openAiService.SetIndexedContents(mapper.Map<IEnumerable<CommentRelatedText>>(contents), asssignmentDescription, referenceText);

        var generatedScore = await openAiService.AssessEssayAsync();

        List<FeedbackComment> comments = new List<FeedbackComment>();

        foreach (var grammarComment in generatedScore.GrammarComments)
        {
            var baseComment = mapper.Map<BaseComment>(grammarComment);
            comments.Add(new FeedbackComment(baseComment, CommentType.Grammar));
        }
        foreach (var eloquenceComment in generatedScore.EloquenceComments)
        {
            var baseComment = mapper.Map<BaseComment>(eloquenceComment);
            comments.Add(new FeedbackComment(baseComment, CommentType.Eloquence));
            
        }
        foreach (var argumentationComment in generatedScore.ArgumentationComments)
        {
            var baseComment = mapper.Map<BaseComment>(argumentationComment);
            comments.Add(new FeedbackComment(baseComment, CommentType.Eloquence));
        }


        var score = new Score
        {
            GrammarScore = generatedScore.GrammarScore,
            GrammarRecommendation = generatedScore.GrammarRecommendation,
            EloquenceScore = generatedScore.EloquenceScore,
            EloquenceRecommendation = generatedScore.EloquenceRecommendation,
            ArgumentationScore = generatedScore.ArgumentationScore,
            ArgumentationRecommendation = generatedScore.ArgumentationRecommendation,
            AssignmentAnswer = generatedScore.AssignmentAnswer,
            AssignmentAnswerScore = generatedScore.AssignmentAnswerScore,
            AssignmentAnswerRecommendation = generatedScore.AssignmentAnswerRecommendation,
            OverallStructure = generatedScore.OverallStructure,
            OverallStructureScore = generatedScore.OverallStructureScore,
            OverallStructureRecommendation = generatedScore.OverallStructureRecommendation
        };

        var returnDto = new ConversationDTO
        {
            EssayScore = score,
            Comments = comments
        };

        return returnDto;

    }

    private void AssignRelatedText(IEnumerable<FeedbackComment> comments, IEnumerable<IndexedContent> indexedContent, int essayId)
    {
        try
        {
            foreach (var feedbackComment in comments)
            {
                var relatedContent = feedbackComment.RelatedText;
                feedbackComment.RelatedText = indexedContent.Single(i => i.ParagraphIndex == relatedContent?.ParagraphIndex
                                                                         && i.RunIndex == relatedContent.RunIndex
                                                                         && i.TextIndex == relatedContent.TextIndex
                                                                         && i.EssayId == essayId);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            foreach (var feedbackComment in comments)
            {
                var relatedContent = feedbackComment.RelatedText;
                var matchingElements = indexedContent.Count(i => i.ParagraphIndex == relatedContent?.ParagraphIndex
                                                                 && i.RunIndex == relatedContent.RunIndex
                                                                 && i.TextIndex == relatedContent.TextIndex
                                                                 && i.EssayId == essayId);
                if (matchingElements == 0)
                {
                    Console.WriteLine($"No matcing elements for OpenAI related content - Paragraph: {relatedContent.ParagraphIndex}, Run: {relatedContent.RunIndex}, Text: {relatedContent.TextIndex}");
                }
            }
            
            Console.WriteLine("Indexes in database");
            foreach (var content in indexedContent)
            {
                var indexes = $"ParagraphIndex: {content.ParagraphIndex.ToString()}, RunIndex: {content.RunIndex.ToString()}, TextIndex: {content.TextIndex}";
                Console.WriteLine(indexes);
            }
            
            Console.WriteLine("Indexes from OpenAI");
            foreach (var comment in comments)
            {
                var indexes = $"ParagraphIndex: {comment.RelatedText.ParagraphIndex.ToString()}, RunIndex: {comment.RelatedText.RunIndex.ToString()}, TextIndex: {comment.RelatedText.TextIndex}";
                Console.WriteLine(indexes);
            }
            
            throw;
        }
        
    }
}