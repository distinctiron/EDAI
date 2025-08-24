using AutoMapper;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentController(EdaiContext context, IMapper _mapper) : ControllerBase
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
    [HttpGet(Name = "GetAssignments")]
    public async Task<IEnumerable<AssignmentDTO>> GetAssignments()
    {
        var orgId = await GetUserOrganisationId();
        var assignments = await context.Assignments
            .Where(a => a.Essays.Any(e => e.Student!.StudentClass.Organisation.OrganisationId == orgId))
            .ToListAsync();
        return _mapper.Map<IEnumerable<AssignmentDTO>>(assignments);
    }

    [Authorize]
    [HttpGet("{id:int}", Name = "GetAssignmentById")]
    public async Task<IResult> GetById(int id)
    {
        var orgId = await GetUserOrganisationId();
        var assignment = await context.Assignments
            .Where(a => a.AssignmentId == id)
            .Where(a => a.Essays.Any(e => e.Student!.StudentClass.Organisation.OrganisationId == orgId))
            .FirstOrDefaultAsync();
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
        var orgId = await GetUserOrganisationId();
        var assignment = await context.Assignments
            .Where(a => a.AssignmentId == id)
            .Where(a => a.Essays.Any(e => e.Student!.StudentClass.Organisation.OrganisationId == orgId))
            .FirstOrDefaultAsync();
        if (assignment == null) return Results.NotFound();
        context.Assignments.Remove(assignment);
        await context.SaveChangesAsync();
        return Results.Ok(assignment);
    }

    [Authorize]
    [HttpPut(Name = "UpdateAssignment")]
    public async Task<IResult> UpdateAssignment(Assignment assignment)
    {
        var orgId = await GetUserOrganisationId();
        var exists = await context.Assignments
            .Where(a => a.AssignmentId == assignment.AssignmentId)
            .AnyAsync(a => a.Essays.Any(e => e.Student!.StudentClass.Organisation.OrganisationId == orgId));
        if (!exists) return Results.NotFound();
        context.Assignments.Update(assignment);
        await context.SaveChangesAsync();
        return Results.Ok(assignment);
    }
    
    [Authorize]
    [HttpPost("{id:int}/uploadDocumentFile", Name = "UploadAssignmentReferenceFile")]
    public async Task<IResult> UploadFile([FromRoute]int id, IFormFile file)
    {
        var orgId = await GetUserOrganisationId();
        var assignment = await context.Assignments
            .Where(a => a.AssignmentId == id)
            .Where(a => a.Essays.Any(e => e.Student!.StudentClass.Organisation.OrganisationId == orgId))
            .FirstOrDefaultAsync();
        if (assignment == null) return Results.NotFound();
        
        MemoryStream memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);

        EdaiDocument edaiDocument = new EdaiDocument
        {
            DocumentName = file.FileName, DocumentFileExtension = file.FileName.Split(".").Last(),
            UploadDate = DateTime.UtcNow
        };
        edaiDocument.DocumentFile = memoryStream.ToArray();

        context.Documents.Add(edaiDocument);
        
        assignment.ReferenceDocument = edaiDocument;
        context.Assignments.Update(assignment);
        
        await context.SaveChangesAsync();

        return Results.Ok(assignment);
    }

    [Authorize]
    [HttpGet("{id:int}/documentFile", Name = "DownloadAssignmentReferenceFile")]
    public async Task<IResult> DownloadFile(int id)
    {
        var orgId = await GetUserOrganisationId();
        var assignment = await context.Assignments
            .Where(a => a.AssignmentId == id)
            .Where(a => a.Essays.Any(e => e.Student!.StudentClass.Organisation.OrganisationId == orgId))
            .FirstOrDefaultAsync();
        if (assignment == null) return Results.NotFound();
        var document = await context.Documents.FindAsync(assignment.ReferenceDocumentId);
        if (document == null) return Results.NotFound();
        var bytes = document.DocumentFile!;
        return Results.File(bytes, "application/octet-stream", document.DocumentName);
    }
}