using AutoMapper;
using EDAI.Server.Data;
using EDAI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentClassController(EdaiContext context, IMapper mapper, IOpenAiService openAiService) : ControllerBase
{
    [Authorize]
    [HttpGet(Name = "GetStudentClasses")]
    public async Task<IEnumerable<StudentClassDTO>> GetStudentClasses()
    {
        var entities = await context.StudentClasses.ToListAsync();

        var studentClasses = mapper.Map<IEnumerable<StudentClassDTO>>(entities);

        return studentClasses;
    }

    [Authorize]
    [HttpGet("{id:int}", Name = "GetStudentClassById")]
    public async Task<IResult> GetById(int id)
    {
        var studentClass = await context.StudentClasses.FindAsync(id);
        return studentClass == null ? Results.NotFound() : Results.Ok(studentClass);
    }

    [Authorize]
    [HttpPost(Name = "AddStudentClass")]
    public async Task<IResult> AddStudentClass(StudentClassDTO studentClassDto)
    {
        var studentClass = mapper.Map<StudentClass>(studentClassDto);
        
        context.StudentClasses.Add(studentClass);
        await context.SaveChangesAsync();
        return Results.Ok(studentClass.StudentClassId);
    }

    [Authorize]
    [HttpDelete("{id:int}", Name = "DeleteStudentClass")]
    public async Task<IResult> DeleteStudent(int id)
    {
        var studentClass = await context.StudentClasses.FindAsync(id);
        if (studentClass == null) return Results.NotFound();
        context.StudentClasses.Remove(studentClass);
        await context.SaveChangesAsync();
        return Results.Ok(studentClass);
    }

    [Authorize]
    [HttpPut(Name = "UpdateStudentClass")]
    public async Task<IResult> UpdateStudent(StudentClassDTO studentClassDto)
    {
        var studentClass = mapper.Map<StudentClass>(studentClassDto);
        
        context.StudentClasses.Update(studentClass);
        await context.SaveChangesAsync();
        return Results.Ok(studentClassDto);
    }
}