using AutoMapper;
using EDAI.Server.Data;
using EDAI.Server.Jobs;
using EDAI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController(EdaiContext context, IMapper mapper, IOpenAiService openAiService) : ControllerBase
{
    [Authorize]
    [HttpGet(Name = "GetStudents")]
    public async Task<IEnumerable<StudentDTO>> GetStudents()
    {
        var entities = await context.Students.Include( s => s.Essays).ToListAsync();

        var students = mapper.Map<IEnumerable<StudentDTO>>(entities);
        foreach (var student in students)
        {
            student.FullName = student.LastName + ", " + student.FirstName;
        }

        return students;
    }

    [Authorize]
    [HttpGet("{id:int}", Name = "GetStudentById")]
    public async Task<IResult> GetById(int id)
    {
        var student = await context.Students.FindAsync(id);
        return student == null ? Results.NotFound() : Results.Ok(student);
    }
    
    [Authorize]
    [HttpPost("generateStudentSummary/{id:int}", Name = "GenerateStudentSummary")]
    public async Task<IResult> GenerateStudentSummary(int id, [FromQuery] string connectionString)
    {
        var jobId = BackgroundJob.Enqueue<IGenerateStudentSummaryService>(s => s.GenerateStudentSummaryScore(id, connectionString));

        var response = new
        {
            Message = "Student summary is being generated",
            JobId = jobId
        };
        
        return Results.Accepted(null,response);
    }
    
    [Authorize]
    [HttpGet("getStudentSummary/{summaryId:int}", Name = "GetStudentSummary")]
    public async Task<IResult> GetStudentSummary(int summaryId)
    {
        var studentSummary = await context.StudentSummaries.SingleAsync(s => s.StudentSummaryId == summaryId);

        var studentSummaryDto = mapper.Map<StudentSummaryDTO>(studentSummary);
        
        return Results.Ok(studentSummaryDto);
    }

    [Authorize]
    [HttpPost(Name = "AddStudent")]
    public async Task<IResult> AddStudent(Student student)
    {
        context.Students.Add(student);
        await context.SaveChangesAsync();
        return Results.Ok(student.StudentId);
    }

    [Authorize]
    [HttpDelete("{id:int}", Name = "DeleteStudent")]
    public async Task<IResult> DeleteStudent(int id)
    {
        var student = await context.Students.FindAsync(id);
        if (student == null) return Results.NotFound();
        context.Students.Remove(student);
        await context.SaveChangesAsync();
        return Results.Ok(student);
    }

    [Authorize]
    [HttpPut(Name = "UpdateStudent")]
    public async Task<IResult> UpdateStudent(Student student)
    {
        context.Students.Update(student);
        await context.SaveChangesAsync();
        return Results.Ok(student);
    }
}