using AutoMapper;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ScoreDTO = EDAI.Shared.Models.Entities.ScoreDTO;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController(EdaiContext context, IMapper _mapper) : ControllerBase
{   
    [Authorize]
    [HttpGet("{id:int}", Name = "GetStudentAnalytics")]
    public async Task<IResult> GetStudentAnalytics(int id)
    {
        var essays = context.Essays.Where(e => e.StudentId == id && e.Scores.Count > 0).ToList();

        var essayAnalyses = GetEssayAnalyses(essays).ToList();

        var student = context.Students.Single(s => s.StudentId == id);

        var studentAnalysis = new StudentAnalysisDTO
        {
            StudentId = id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            StudentClass = student.Class,
            EssayAnalysese = essayAnalyses
        };
        
        return Results.Ok(studentAnalysis);
    }
    
    [Authorize]
    [HttpGet("class/{id:int}", Name = "GetClassAnalytics")]
    public async Task<IResult> GetClassAnalytics(int id)
    {
        var students = await context.Students.Where(s => s.StudentClassId == id).ToListAsync();
        
        var studentAnalysisDtos = new List<StudentAnalysisDTO>();

        foreach (var student in students)
        {
            studentAnalysisDtos.Add( new StudentAnalysisDTO
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                StudentClass = student.Class,
                EssayAnalysese = GetEssayAnalyses(student).ToList()
            });
        }
        
        var classAnalysis = new ClassAnalysisDTO
        {
            ClassId = id,
            StudentAnalysisDtos = studentAnalysisDtos
        };
        
        return Results.Ok(classAnalysis);
    }
    
    private IEnumerable<EssayAnalysisDTO> GetEssayAnalyses(Student student)
    {
        var essays = context.Essays.Where(e => e.StudentId == student.StudentId && e.Scores.Count > 0).ToList();
        return GetEssayAnalyses(essays).ToList();
    }
    
    private IEnumerable<EssayAnalysisDTO> GetEssayAnalyses(IEnumerable<Essay> essays)
    {
        foreach (var essay in essays)
        {
            if (context.Scores.Count(s => s.EssayId == essay.EssayId) > 0)
            {
                yield return GetEssayAnalysis(essay);
            }
        }
    }
    
    private  EssayAnalysisDTO GetEssayAnalysis(Essay essay)
    {
        return new EssayAnalysisDTO
        {
            EssayId = essay.EssayId,
            AssignmentName = context.Assignments.Single(a => a.AssignmentId == essay.AssignmentId).Name,
            EssayTitle = context.Documents.Single( d => d.EdaiDocumentId == essay.EdaiDocumentId).DocumentName,
            Score = _mapper.Map<ScoreDTO>(context.Scores.Single(s => s.EssayId == essay.EssayId)),
            Comments = _mapper.Map<IEnumerable<FeedbackCommentDTO>>(context.FeedbackComments.Where(c => c.RelatedTexts.Any(ic => ic.EssayId == essay.EssayId)).ToList()) 
        };
    }

}