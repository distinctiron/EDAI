using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AssignmentController(EdaiContext context) : ControllerBase
{
    [HttpGet(Name = "GetAssignments")]
    public IEnumerable<Assignment> GetAssignments()
    {
        return context.Assignments;
    }

    [HttpGet("{id:int}", Name = "GetAssignmentById")]
    public IResult GetById(int id)
    {
        var assignment = context.Assignments.Find(id);
        return assignment == null ? Results.NotFound() : Results.Ok(assignment);
    }

    [HttpPost(Name = "AddAssignment")]
    public IResult AddAssignment(Assignment assignment)
    {
        context.Assignments.Add(assignment);
        context.SaveChanges();
        return Results.Created();
    }

    [HttpDelete("{id:int}", Name = "DeleteAssignment")]
    public IResult DeleteAssignment(int id)
    {
        var assignment = context.Assignments.Find(id);
        if (assignment == null) return Results.NotFound();
        context.Assignments.Remove(assignment);
        context.SaveChanges();
        return Results.Ok(assignment);
    }

    [HttpPut(Name = "UpdateAssignment")]
    public IResult UpdateAssignment(Assignment assignment)
    {
        context.Assignments.Update(assignment);
        context.SaveChanges();
        return Results.Ok(assignment);
    }
}