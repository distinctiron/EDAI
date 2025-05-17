using AutoMapper;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AnalyticsController(EdaiContext context, IMapper _mapper) : ControllerBase
{   
    [HttpGet("{id:int}", Name = "GetStudentAnalytics")]
    public IResult GetStudentAnalytics(int id)
    {
        var essays = context.Essays.Where(e => e.StudentId == id);

        var essayAnalyses = GetEssayAnalyses(essays);

        /*var rto = new List<EssayAnalysisDTO>();

        foreach (var essay in essays)
        {
            if (context.Scores.Count(s => s.EssayId == essay.EssayId) > 1)
            {
                Console.WriteLine($"EssayId: {essay.EssayId} with {context.Scores.Count(s => s.EssayId == essay.EssayId)} Scores");   
            } 
        }*/

        var studentAnalysis = new StudentAnalysisDTO
        {
            StudentId = id,
            EssayAnalysese = essayAnalyses
        };
        
        return Results.Ok(studentAnalysis);
    }

    private IEnumerable<EssayAnalysisDTO> GetEssayAnalyses(IEnumerable<Essay> essays)
    {
        foreach (var essay in essays)
        {
            if (context.Scores.Count(s => s.EssayId == essay.EssayId) > 0)
            {
                yield return new EssayAnalysisDTO
                {
                    EssayId = essay.EssayId,
                    EssayTitle = context.Documents.Single( d => d.EdaiDocumentId == essay.EdaiDocumentId).DocumentName,
                    Score = context.Scores.Single(s => s.EssayId == essay.EssayId),
                    Comments = context.FeedbackComments.Where(c => c.RelatedTexts.Any(ic => ic.EssayId == essay.EssayId))
                };
            }
        }
    }
}