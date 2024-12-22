using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EssayController(EdaiContext context) : ControllerBase
{
    [HttpGet(Name = "GetEssays")]
    public IEnumerable<Essay> GetEssays()
    {
        return context.Essays;
    }

    [HttpGet("{id:int}", Name = "GetEssaysById")]
    public IResult GetById(int id)
    {
        var essay = context.Essays.Find(id);
        return essay == null ? Results.NotFound() : Results.Ok(essay);
    }

    [HttpPost(Name = "AddEssay")]
    public IResult AddEssay(Essay essay)
    {
        context.Essays.Add(essay);
        context.SaveChanges();
        return Results.Created();
    }

    [HttpDelete("{id:int}", Name = "DeleteEssay")]
    public IResult DeleteEssay(int id)
    {
        var essay = context.Essays.Find(id);
        if (essay == null) return Results.NotFound();
        context.Essays.Remove(essay);
        context.SaveChanges();
        return Results.Ok(essay);
    }

    [HttpPut(Name = "UpdateEssay")]
    public IResult UpdateEssay(Essay essay)
    {
        context.Essays.Update(essay);
        context.SaveChanges();
        return Results.Ok(essay);
    }
}