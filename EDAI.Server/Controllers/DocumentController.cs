using System.Text.Json;
using AutoMapper;
using EDAI.Server.Data;
using Microsoft.AspNetCore.Mvc;
using EDAI.Shared.Models;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController(EdaiContext context, IMapper _mapper) : ControllerBase
{
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

        context.SaveChanges();

        return Ok(entity.EdaiDocumentId);
    }
}