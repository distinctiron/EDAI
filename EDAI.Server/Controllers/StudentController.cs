using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared;
using EDAI.Shared.Models;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentController(EdaiContext context) : ControllerBase
{
    [HttpGet(Name = "GetStudents")]
    public IEnumerable<Student> GetStudents()
    {
        return context.Students;
    }

    [HttpGet("id:int", Name = "GetStudentById")]
    public IResult GetById(int id)
    {
        var student = context.Students.Find(id);
        return student == null ? Results.NotFound() : Results.Ok(student);
    }

    [HttpPost(Name = "AddStudent")]
    public IResult AddStudent(Student student)
    {
        context.Students.Add(student);
        context.SaveChanges();
        return Results.Ok(student.StudentId);
    }

    [HttpDelete("id:int", Name = "DeleteStudent")]
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