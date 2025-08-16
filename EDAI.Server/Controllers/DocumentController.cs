using AutoMapper;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController(EdaiContext context, IMapper _mapper) : ControllerBase
{
    [Authorize]
    [HttpPost("uploadFiles",Name = "UploadFiles")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var entity = new EdaiDocument
        {
            DocumentName = Path.GetFileNameWithoutExtension(file.FileName),
            DocumentFileExtension = Path.GetExtension(file.FileName),
            DocumentFile = ms.ToArray(),
            UploadDate = DateTime.UtcNow
        };

        
        context.Documents.Add(entity);

        await context.SaveChangesAsync();

        return Ok(entity.EdaiDocumentId);
    }
}