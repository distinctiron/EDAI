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
using System.Security.Claims;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController(EdaiContext context, IMapper mapper, IOpenAiService openAiService) : ControllerBase
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
    [HttpGet(Name = "GetStudents")]
    public async Task<IEnumerable<StudentDTO>> GetStudents()
    {
        var orgId = await GetUserOrganisationId();
        var entities = await context.Students
            .Include(s => s.Essays)
            .Include(s => s.StudentClass)
            .ThenInclude(sc => sc.Organisation)
            .Where(s => s.StudentClass.Organisation.OrganisationId == orgId)
            .ToListAsync();

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
        var orgId = await GetUserOrganisationId();
        var student = await context.Students
            .Include(s => s.StudentClass)
            .ThenInclude(sc => sc.Organisation)
            .SingleOrDefaultAsync(s => s.StudentId == id && s.StudentClass.Organisation.OrganisationId == orgId);
        return student == null ? Results.NotFound() : Results.Ok(student);
    }
    
    [Authorize]
    [HttpPost("generateStudentSummary/{id:int}", Name = "GenerateStudentSummary")]
    public async Task<IResult> GenerateStudentSummary(int id, [FromQuery] string connectionString)
    {
        var orgId = await GetUserOrganisationId();
        var studentExists = await context.Students
            .Include(s => s.StudentClass)
            .ThenInclude(sc => sc.Organisation)
            .AnyAsync(s => s.StudentId == id && s.StudentClass.Organisation.OrganisationId == orgId);
        if (!studentExists) return Results.NotFound();

        var jobId = BackgroundJob.Enqueue<IGenerateStudentSummaryService>(s => s.GenerateStudentSummaryScore(id, connectionString));

        var response = new
        {
            Message = "Student summary is being generated",
            JobId = jobId
        };

        return Results.Accepted(null, response);
    }
    
    [Authorize]
    [HttpGet("getStudentSummary/{summaryId:int}", Name = "GetStudentSummary")]
    public async Task<IResult> GetStudentSummary(int summaryId)
    {
        var orgId = await GetUserOrganisationId();
        var studentSummary = await context.StudentSummaries
            .Include(ss => ss.Student)!.ThenInclude(s => s.StudentClass)
            .ThenInclude(sc => sc.Organisation)
            .SingleOrDefaultAsync(s => s.StudentSummaryId == summaryId && s.Student!.StudentClass.Organisation.OrganisationId == orgId);

        if (studentSummary == null) return Results.NotFound();

        var studentSummaryDto = mapper.Map<StudentSummaryDTO>(studentSummary);

        return Results.Ok(studentSummaryDto);
    }

    [Authorize]
    [HttpPost(Name = "AddStudent")]
    public async Task<IResult> AddStudent(Student student)
    {
        var orgId = await GetUserOrganisationId();
        var studentClass = await context.StudentClasses
            .Include(sc => sc.Organisation)
            .SingleOrDefaultAsync(sc => sc.StudentClassId == student.StudentClassId && sc.Organisation.OrganisationId == orgId);
        if (studentClass == null) return Results.Forbid();

        context.Students.Add(student);
        await context.SaveChangesAsync();
        return Results.Ok(student.StudentId);
    }

    [Authorize]
    [HttpDelete("{id:int}", Name = "DeleteStudent")]
    public async Task<IResult> DeleteStudent(int id)
    {
        var orgId = await GetUserOrganisationId();
        var student = await context.Students
            .Include(s => s.StudentClass)
            .ThenInclude(sc => sc.Organisation)
            .SingleOrDefaultAsync(s => s.StudentId == id && s.StudentClass.Organisation.OrganisationId == orgId);
        if (student == null) return Results.NotFound();
        context.Students.Remove(student);
        await context.SaveChangesAsync();
        return Results.Ok(student);
    }

    [Authorize]
    [HttpPut(Name = "UpdateStudent")]
    public async Task<IResult> UpdateStudent(Student student)
    {
        var orgId = await GetUserOrganisationId();
        var exists = await context.Students
            .Include(s => s.StudentClass)
            .ThenInclude(sc => sc.Organisation)
            .AnyAsync(s => s.StudentId == student.StudentId && s.StudentClass.Organisation.OrganisationId == orgId);
        if (!exists) return Results.NotFound();
        context.Students.Update(student);
        await context.SaveChangesAsync();
        return Results.Ok(student);
    }
}