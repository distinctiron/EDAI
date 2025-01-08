using AutoMapper;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AssignmentController(EdaiContext context, IMapper _mapper) : ControllerBase
{   
    [HttpGet(Name = "GetAssignments")]
    public IEnumerable<AssignmentDTO> GetAssignments()
    {
        return _mapper.Map<IEnumerable<AssignmentDTO>>(context.Assignments);
    }

    [HttpGet("{id:int}", Name = "GetAssignmentById")]
    public IResult GetById(int id)
    {
        var assignment = context.Assignments.Find(id);
        return assignment == null ? Results.NotFound() : Results.Ok(assignment);
    }

    [HttpPost(Name = "AddAssignment")]
    public IResult AddAssignment(AssignmentDTO assignment)
    {
        context.Assignments.Add(_mapper.Map<Assignment>(assignment));
        context.SaveChanges();
        return Results.Ok(assignment.AssignmentId);
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
    
    [HttpPost("{id:int}/uploadDocumentFile", Name = "UploadAssignmentReferenceFile")]
    public IResult UploadFile([FromRoute]int id, IFormFile file)
    {
        var assignment = context.Assignments.Find(id);
        if (assignment == null) return Results.NotFound();
        
        MemoryStream memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);

        EdaiDocument edaiDocument = new EdaiDocument
        {
            DocumentName = file.FileName, DocumentFileExtension = file.FileName.Split(".").Last(),
            UploadDate = DateTime.Now
        };
        edaiDocument.DocumentFile = memoryStream.ToArray();

        context.Documents.Add(edaiDocument);
        
        assignment.ReferenceDocument = edaiDocument;
        context.Assignments.Update(assignment);
        
        context.SaveChanges();

        return Results.Ok(assignment);
    }

    [HttpGet("{id:int}/documentFile", Name = "DownloadAssignmentReferenceFile")]
    public IResult DownloadFile(int id)
    {
        var assignment = context.Assignments.Find(id);
        if (assignment == null) return Results.NotFound();
        var document = context.Documents.Find(assignment.ReferenceDocumentId);
        if (document == null) return Results.NotFound();
        var bytes = document.DocumentFile!;
        return Results.File(bytes, "application/octet-stream", document.DocumentName);
    }
}