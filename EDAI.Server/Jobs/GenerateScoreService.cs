using AutoMapper;
using EDAI.Server.Data;
using EDAI.Server.Hubs;
using EDAI.Services.Interfaces;
using EDAI.Shared.Factories;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;
using EDAI.Shared.Tools;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Jobs;

public class GenerateScoreService(IWordFileHandlerFactory wordFileHandlerFactory, IOpenAiService openAiService, IMapper mapper, IHubContext<MessageHub> messageHub, IServiceProvider serviceProvider) : IGenerateScoreService
{
    public async Task GenerateScore(IEnumerable<int> documentIds, string connectionId)
    {

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EdaiContext>();
            
            var documents = await context.Documents.Where(d => documentIds.Contains(d.EdaiDocumentId)).ToListAsync();
        
            var essays = await context.Essays.Where(e => documentIds.Contains(e.EdaiDocumentId)).ToListAsync();

            var assignmentIds = essays.Select(e => e.AssignmentId).Distinct();

            var assignments = await context.Assignments.Where(a => assignmentIds.Contains(a.AssignmentId)).ToListAsync();

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

                    if (await context.Scores.AnyAsync(s => s.EssayId == essayId))
                    {
                        continue;
                    }

                    using (var documentStream = new MemoryStream(document.DocumentFile))
                    {
                        using (var wordFileHandler =
                               wordFileHandlerFactory.CreateWordFileHandler(documentStream, essayId))
                        {
                            var essayText = wordFileHandler.GetDocumentText();

                            openAiService.InitiateScoreConversation(essayText, assignment.Description, referenceText);

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

                    }

                    await context.SaveChangesAsync();

                    //await messageHub.Clients.Client(connectionId).SendAsync("ScoreGenerated",essayId.ToString());
                    await messageHub.Clients.All.SendAsync("ScoreGenerated", essayId.ToString());
                }
            
            }

        }
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
}