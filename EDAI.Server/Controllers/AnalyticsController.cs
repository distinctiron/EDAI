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
        var essays = context.Essays.Where(e => e.StudentId == id && e.Scores.Count > 0);

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
            FirstName = context.Students.Single(s => s.StudentId == id).FirstName,
            LastName = context.Students.Single(s => s.StudentId == id).LastName,
            StudentClass = context.Students.Single(s => s.StudentId == id).Class,
            EssayAnalysese = essayAnalyses
        };
        
        return Results.Ok(studentAnalysis);
    }

    [HttpGet("class/{id:int}", Name = "GetClassAnalytics")]
    public IResult GetClassAnalytics(int id)
    {
        var students = context.Students.Where(s => s.StudentClassId == id);
        
        var studentAnalysisDtos = new List<StudentAnalysisDTO>();

        foreach (var student in students)
        {
            studentAnalysisDtos.Add( new StudentAnalysisDTO
                {
                    StudentId = student.StudentId,
                    FirstName = context.Students.Single(s => s.StudentId == student.StudentId).FirstName,
                    LastName = context.Students.Single(s => s.StudentId == student.StudentId).LastName,
                    StudentClass = context.Students.Single(s => s.StudentId == student.StudentId).Class,
                    EssayAnalysese = GetEssayAnalyses(student)
                });
        }
        
        var classAnalysis = new ClassAnalysisDTO
        {
            ClassId = id,
            StudentAnalysisDtos = studentAnalysisDtos
        };
        
        return Results.Ok(classAnalysis);
    }

    private EssayAnalysisDTO GetEssayAnalysis(Essay essay)
    {
        return new EssayAnalysisDTO
        {
            EssayId = essay.EssayId,
            AssignmentName = context.Assignments.Single(a => a.AssignmentId == essay.AssignmentId).Name,
            EssayTitle = context.Documents.Single( d => d.EdaiDocumentId == essay.EdaiDocumentId).DocumentName,
            Score = context.Scores.Single(s => s.EssayId == essay.EssayId),
            Comments = context.FeedbackComments.Where(c => c.RelatedTexts.Any(ic => ic.EssayId == essay.EssayId))
        };
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

    private IEnumerable<EssayAnalysisDTO> GetEssayAnalyses(Student student)
    {
        var essays = context.Essays.Where(e => e.StudentId == student.StudentId && e.Scores.Count > 0);
        return GetEssayAnalyses(essays);
    }
}