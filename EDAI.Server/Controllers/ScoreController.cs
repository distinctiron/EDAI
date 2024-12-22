using DocumentFormat.OpenXml.Office2010.Excel;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ScoreController(EdaiContext context) : ControllerBase
{
    [HttpGet(Name = "GetScores")]
    public IEnumerable<Score> GetScores()
    {
        return context.Scores;
    }

    [HttpGet("id:int", Name = "GetScoresById")]
    public IResult GetById(int id)
    {
        var score = context.Scores.Find(id);
        return score == null ? Results.NotFound() : Results.Ok(score);
    }

    [HttpPost(Name = "AddScore")]
    public IResult AddScore(Score score)
    {
        context.Scores.Add(score);
        context.SaveChanges();
        return Results.Created();
    }

    [HttpDelete("id:int", Name = "DeleteScore")]
    public IResult DeleteScore(int id)
    {
        var score = context.Scores.Find(id);
        if (score == null) return Results.NotFound();
        context.Scores.Remove(score);
        context.SaveChanges();
        return Results.Ok(score);
    }

    [HttpPut(Name = "UpdateScore")]
    public IResult UpdateScore(Score score)
    {
        context.Scores.Update(score);
        context.SaveChanges();
        return Results.Ok(score);
    }
}