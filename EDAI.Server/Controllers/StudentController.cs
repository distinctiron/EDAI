using AutoMapper;
using EDAI.Client.Pages;
using EDAI.Server.Data;
using EDAI.Server.Jobs;
using EDAI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared;
using EDAI.Shared.Models;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentController(EdaiContext context, IMapper mapper, IOpenAiService openAiService) : ControllerBase
{
    [HttpGet(Name = "GetStudents")]
    public IEnumerable<StudentDTO> GetStudents()
    {
        var entities = context.Students.Include( s => s.Essays);

        var students = mapper.Map<IEnumerable<StudentDTO>>(entities);
        foreach (var student in students)
        {
            student.FullName = student.LastName + ", " + student.FirstName;
        }

        return students;
    }

    [HttpGet("{id:int}", Name = "GetStudentById")]
    public IResult GetById(int id)
    {
        var student = context.Students.Find(id);
        return student == null ? Results.NotFound() : Results.Ok(student);
    }
    
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
    
    [HttpGet("getStudentSummary/{summaryId:int}", Name = "GetStudentSummary")]
    public async Task<IResult> GetStudentSummary(int summaryId)
    {
        var studentSummary = context.StudentSummaries.Single(s => s.StudentSummaryId == summaryId);

        var studentSummaryDto = mapper.Map<StudentSummaryDTO>(studentSummary);
        
        return Results.Ok(studentSummaryDto);
    }

    [HttpGet("getClassName/{classId:int}", Name = "GetClassName")]
    public IResult GetClassName(int classId)
    {
        var studentClass = context.StudentClasses.Single(c => c.StudentClassId == classId);

        return Results.Ok(studentClass.Class + ", " + studentClass.School);
    }

    [HttpPost(Name = "AddStudent")]
    public IResult AddStudent(Student student)
    {
        context.Students.Add(student);
        context.SaveChanges();
        return Results.Ok(student.StudentId);
    }

    [HttpDelete("{id:int}", Name = "DeleteStudent")]
    public IResult DeleteStudent(int id)
    {
        var student = context.Students.Find(id);
        if (student == null) return Results.NotFound();
        context.Students.Remove(student);
        context.SaveChanges();
        return Results.Ok(student);
    }

    [HttpPut(Name = "UpdateStudent")]
    public IResult UpdateStudent(Student student)
    {
        context.Students.Update(student);
        context.SaveChanges();
        return Results.Ok(student);
    }
}