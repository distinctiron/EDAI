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
    public async Task<IResult> GenerateScores(IEnumerable<int> documentIds)
    {

        BackgroundJob.Enqueue<IGenerateScoreService>(s => s.GenerateScore(documentIds));
        
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
        
            foreach (var document in documents)
            {
                var essayId = essays.Where(e => e.EdaiDocumentId == document.EdaiDocumentId).Select(e => e.EssayId).Single();

                using (var wordFileHandler =
                       wordFileHandlerFactory.CreateWordFileHandler(new MemoryStream(document.DocumentFile), essayId))
                {
                    var essayText = wordFileHandler.GetDocumentText();
                    
                    openAiService.InitiateConversation(essayText, assignment.Description, referenceText);
                    
                    var generatedScore = await openAiService.AssessEssayAsync();

                    var reviewTuple = await wordFileHandler.CreateReviewDocument(document, generatedScore);

                    var indexedContents = reviewTuple.Item2;
                    
                    context.IndexedContents.AddRange(indexedContents);

                    var reviewedDocument = reviewTuple.Item1;
                    context.Documents.Add(reviewedDocument);
                    
                    var outputEntities = OutputEntitiesFromAI(generatedScore, indexedContents);

                    var score = outputEntities.Item1;
                    score.EvaluatedEssayDocument = reviewedDocument;
                    score.EvaluatedEssayDocumentId = reviewedDocument.EdaiDocumentId;
                    score.Essay = essays.Single(e => e.EdaiDocumentId == document.EdaiDocumentId);
                    score.EssayId = essayId;

                    context.Scores.Add(score);
                    
                    var feedbackComments = outputEntities.Item2;
                    context.FeedbackComments.AddRange(feedbackComments);
                    
                }
                
                /*
                using (MemoryStream documentStream = new MemoryStream(document.DocumentFile))
                {
                    using MemoryStream reviewedDocumentStream = new MemoryStream();
                    await documentStream.CopyToAsync(reviewedDocumentStream);

                    var wordFileHandler = wordFileHandlerFactory.CreateWordFileHandler(reviewedDocumentStream, essayId);
                
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
                    wordFileHandler.InsertFeedback(reviewedDocumentStream, score.OverallStructure);
                    wordFileHandler.InsertFeedback(reviewedDocumentStream, score.AssignmentAnswer);

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
                */
            }
            
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
        return Results.File(bytes, "application/octet-stream", document.DocumentName + document.DocumentFileExtension);
    }

    private (Score, IEnumerable<FeedbackComment>) OutputEntitiesFromAI(GenerateScoreDTO generatedScore, IEnumerable<IndexedContent> indexedContents)
    {
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
        
        var comments = new List<FeedbackComment>();

        foreach (var grammarComment in generatedScore.GrammarComments)
        {
            comments.Add(GetFeedbackComment(grammarComment, indexedContents, CommentType.Grammar));
        }
        foreach (var eloquenceComment in generatedScore.EloquenceComments)
        {
            comments.Add(GetFeedbackComment(eloquenceComment, indexedContents, CommentType.Eloquence));
        }
        foreach (var argumentationComment in generatedScore.ArgumentationComments)
        {
            comments.Add(GetFeedbackComment(argumentationComment, indexedContents, CommentType.Logic));
        }

        return (score, comments);
    }

    private FeedbackComment GetFeedbackComment(CommentDTO comment, IEnumerable<IndexedContent> contents, CommentType commentType)
    {
        var text = string.Join("", contents.
            OrderBy(c => c.ParagraphIndex).
            ThenBy(c => c.RunIndex).
            Select(c => c.Content));

        var startIndex = text.IndexOf(comment.RelatedText);
        var endIndex = startIndex + comment.RelatedText.Length;
        
        var indexedContents = contents.Where(c => c.FromCharInContent >= startIndex && c.ToCharInContent <= endIndex);

        try
        {
            var baseComment = mapper.Map<BaseComment>(comment);
            baseComment.RelatedTexts = indexedContents.ToList();
            return new FeedbackComment(baseComment, commentType);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine($"Multiple indexedContent for comment {comment.CommentFeedback}");
            Console.WriteLine(e);
            throw;
        }
    }
/*
    private async Task<(Score, IEnumerable<FeedbackComment>)> OpenAIConversor(string essayText, string asssignmentDescription, string referenceText)
    {
        openAiService.InitiateConversation(essayText, asssignmentDescription, referenceText);

        var generatedScore = await openAiService.AssessEssayAsync();
        


        var comments = new System.Collections.Generic.List<EDAI.Shared.Models.DTO.OpenAI.CommentDTO>();

        foreach (var grammarComment in generatedScore.GrammarComments)
        {
            assignCharPositions(essayText, grammarComment);
            grammarComment.CommentType = CommentType.Grammar.ToString();
            //var baseComment = mapper.Map<BaseComment>(grammarComment);
            comments.Add(grammarComment);
                //new FeedbackComment(baseComment, CommentType.Grammar));
        }
        foreach (var eloquenceComment in generatedScore.EloquenceComments)
        {
            assignCharPositions(essayText, eloquenceComment);
            eloquenceComment.CommentType = CommentType.Eloquence.ToString();
            //var baseComment = mapper.Map<BaseComment>(eloquenceComment);
            comments.Add(eloquenceComment);
            //comments.Add(new FeedbackComment(baseComment, CommentType.Eloquence));

        }
        foreach (var argumentationComment in generatedScore.ArgumentationComments)
        {
            assignCharPositions(essayText, argumentationComment);
            argumentationComment.CommentType = CommentType.Logic.ToString();
            //var baseComment = mapper.Map<BaseComment>(argumentationComment);
            comments.Add(argumentationComment);
            //comments.Add(new FeedbackComment(baseComment, CommentType.Eloquence));
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
            Comments = new CommentsDTO()
            {
                Comments = comments
            }
        };

        return returnDto;

    }
*/
}