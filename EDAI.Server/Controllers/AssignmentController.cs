using AutoMapper;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentController(EdaiContext context, IMapper _mapper) : ControllerBase
{   
    [Authorize]
    [HttpGet(Name = "GetAssignments")]
    public async Task<IEnumerable<AssignmentDTO>> GetAssignments()
    {
        return _mapper.Map<IEnumerable<AssignmentDTO>>(await context.Assignments.ToListAsync());
    }

    [Authorize]
    [HttpGet("{id:int}", Name = "GetAssignmentById")]
    public async Task<IResult> GetById(int id)
    {
        var assignment = await context.Assignments.FindAsync(id);
        return assignment == null ? Results.NotFound() : Results.Ok(assignment);
    }

    [Authorize]
    [HttpPost(Name = "AddAssignment")]
    public async Task<IResult> AddAssignment(AssignmentDTO assignment)
    {
        var entity = _mapper.Map<Assignment>(assignment);
        
        context.Assignments.Add(entity);
        
        await context.SaveChangesAsync();

        /*
        var students = await context.Students.Where(x => assignment.StudentClasses.Contains(x.Class)).ToListAsync();
        
        foreach (var student in students)
        {
            context.Essays.Add(new Essay
            {
                AssignmentId = entity.AssignmentId,
                
                Evaluated = false,
                Student = student
            });
        }

        await context.SaveChangesAsync();*/
        
        return Results.Ok(entity.AssignmentId);
    }

    [Authorize]
    [HttpDelete("{id:int}", Name = "DeleteAssignment")]
    public async Task<IResult> DeleteAssignment(int id)
    {
        var assignment = await context.Assignments.FindAsync(id);
        if (assignment == null) return Results.NotFound();
        context.Assignments.Remove(assignment);
        await context.SaveChangesAsync();
        return Results.Ok(assignment);
    }

    [Authorize]
    [HttpPut(Name = "UpdateAssignment")]
    public async Task<IResult> UpdateAssignment(Assignment assignment)
    {
        context.Assignments.Update(assignment);
        await context.SaveChangesAsync();
        return Results.Ok(assignment);
    }
    
    [Authorize]
    [HttpPost("{id:int}/uploadDocumentFile", Name = "UploadAssignmentReferenceFile")]
    public async Task<IResult> UploadFile([FromRoute]int id, IFormFile file)
    {
        var assignment = await context.Assignments.FindAsync(id);
        if (assignment == null) return Results.NotFound();
        
        MemoryStream memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);

        EdaiDocument edaiDocument = new EdaiDocument
        {
            DocumentName = file.FileName, DocumentFileExtension = file.FileName.Split(".").Last(),
            AssignmentId = id,
            UploadDate = DateTime.UtcNow
        };
        edaiDocument.DocumentFile = memoryStream.ToArray();

        context.Documents.Add(edaiDocument);
        
        context.Assignments.Update(assignment);
        
        await context.SaveChangesAsync();

        return Results.Ok(assignment);
    }
/*
    [Authorize]
    [HttpGet("{id:int}/documentFile", Name = "DownloadAssignmentReferenceFile")]
    public async Task<IResult> DownloadFile(int id)
    {
        var assignment = await context.Assignments.FindAsync(id);
        if (assignment == null) return Results.NotFound();
        var document = await context.Documents.FindAsync(assignment.ReferenceDocumentId);
        if (document == null) return Results.NotFound();
        var bytes = document.DocumentFile!;
        return Results.File(bytes, "application/octet-stream", document.DocumentName);
    } */
}