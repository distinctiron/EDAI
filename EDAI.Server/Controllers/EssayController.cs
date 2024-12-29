using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EssayController(EdaiContext context) : ControllerBase
{
    [HttpGet(Name = "GetEssays")]
    public IEnumerable<Essay> GetEssays()
    {
        return context.Essays
            .Include( e => e.Assignment)
            .Include(e => e.Student)
            .Include(e => e.Score);
    }

    [HttpGet("{id:int}", Name = "GetEssaysById")]
    public Essay? GetById(int id)
    {
        var essay = context.Essays
            .Include(e => e.Assignment)
            .Include(e => e.Student)
            .Include(e => e.Score)
            .SingleOrDefault(e => e.EssayId == id);
        return essay;
    }

    [HttpPost(Name = "AddEssay")]
    public IResult AddEssay(Essay essay)
    {
        context.Essays.Add(essay);
        context.SaveChanges();
        return Results.Ok(essay.EssayId);
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

    [HttpPost("{id:int}", Name = "UploadFile")]
    public IResult UploadFile([FromRoute]int id, IFormFile file)
    {
        var essay = context.Essays.Find(id);
        if (essay == null) return Results.NotFound();
        
        MemoryStream memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        essay.File = memoryStream.ToArray();
        context.SaveChanges();

        return Results.Ok(essay);
    }

    [HttpGet("{id:int}/file", Name = "DownloadFile")]
    public IResult DownloadFile(int id)
    {
        var essay = context.Essays.Find(id);
        if (essay == null) return Results.NotFound();
        var bytes = context.Essays.Find(id)!.File!;
        return Results.File(bytes, "application/octet-stream", "essayName.pdf");
    }
}