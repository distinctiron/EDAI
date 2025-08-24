using AutoMapper;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController(EdaiContext context, IMapper _mapper) : ControllerBase
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
    [HttpGet("{id:int}", Name = "GetStudentAnalytics")]
    public async Task<IResult> GetStudentAnalytics(int id)
    {
        var orgId = await GetUserOrganisationId();
        var studentExists = await context.Students
            .Include(s => s.StudentClass).ThenInclude(sc => sc.Organisation)
            .AnyAsync(s => s.StudentId == id && s.StudentClass.Organisation.OrganisationId == orgId);
        if (!studentExists) return Results.NotFound();

        var essays = await context.Essays
            .Include(e => e.Student)!.ThenInclude(s => s.StudentClass).ThenInclude(sc => sc.Organisation)
            .Where(e => e.StudentId == id && e.Student!.StudentClass.Organisation.OrganisationId == orgId)
            .ToListAsync();

        var essayAnalyses = GetEssayAnalyses(essays);

        IEnumerable<EssayAnalysisDTO> essayAnalysisDtos = new List<EssayAnalysisDTO>();

        await foreach (var essayAnalysis in essayAnalyses)
        {
            essayAnalysisDtos.Append(essayAnalysis);
        }

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
            EssayAnalysese = essayAnalysisDtos
        };
        
        return Results.Ok(studentAnalysis);
    }

    private async IAsyncEnumerable<EssayAnalysisDTO> GetEssayAnalyses(IEnumerable<Essay> essays)
    {
        foreach (var essay in essays)
        {
            if (await context.Scores.CountAsync(s => s.EssayId == essay.EssayId) > 0)
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