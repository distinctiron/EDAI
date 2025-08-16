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
public class EssayController(EdaiContext context, IMapper _mapper) : ControllerBase
{
    [Authorize]
    [HttpGet(Name = "GetEssays")]
    public async Task<IEnumerable<Essay>> GetEssays()
    {
        return await context.Essays
            .Include(e => e.Assignment) 
            .Include(e => e.Student)
            .Include(e => e.IndexedEssay)
            .Include(e => e.Scores).ToListAsync();
    }

    [Authorize]
    [HttpGet("{id:int}", Name = "GetEssaysById")]
    public async Task<Essay?> GetById(int id)
    {
        var essay = await context.Essays
            .Include(e => e.Assignment)
            .Include(e => e.Student)
            .Include(e => e.Scores)
            .Include(e => e.IndexedEssay)
            .SingleOrDefaultAsync(e => e.EssayId == id);
        return essay;
    }

    [Authorize]
    [HttpPost(Name = "AddEssay")]
    public async Task<IResult> AddEssay(Essay essay)
    {
        context.Essays.Add(essay);
        await context.SaveChangesAsync();
        return Results.Ok(essay.EssayId);
    }

    [Authorize]
    [HttpPost("bulk", Name = "BulkAddEssay")]
    public async Task<IResult> BulkAddEssay(EssayFileDTO essay)
    {
        
        var entity = _mapper.Map<Essay>(essay);
        entity.StudentId = entity.Student.StudentId;
        entity.Student = null;
        context.Essays.Add(entity);
        await context.SaveChangesAsync();

        essay.EssayId = entity.EssayId;


        return Results.Ok(essay);
    }

    [Authorize]
    [HttpDelete("{id:int}", Name = "DeleteEssay")]
    public async Task<IResult> DeleteEssay(int id)
    {
        var essay = await context.Essays.FindAsync(id);
        if (essay == null) return Results.NotFound();
        context.Essays.Remove(essay);
        await context.SaveChangesAsync();
        return Results.Ok(essay);
    }

    [Authorize]
    [HttpPut(Name = "UpdateEssay")]
    public async Task<IResult> UpdateEssay(Essay essay)
    {
        context.Essays.Update(essay);
        await context.SaveChangesAsync();
        return Results.Ok(essay);
    }
    
    [Authorize]
    [HttpPost("{id:int}/uploadDocumentFile", Name = "UploadEssayDocumentFile")]
    public async Task<IResult> UploadFile([FromRoute]int id, IFormFile file)
    {
        var essay = await context.Essays.FindAsync(id);
        if (essay == null) return Results.NotFound();
        
        MemoryStream memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);

        EdaiDocument edaiDocument = new EdaiDocument
        {
            DocumentName = file.FileName, DocumentFileExtension = file.FileName.Split(".").Last(),
            UploadDate = DateTime.Now
        };
        edaiDocument.DocumentFile = memoryStream.ToArray();

        context.Documents.Add(edaiDocument);
        
        essay.Document = edaiDocument;
        context.Essays.Update(essay);
        
        await context.SaveChangesAsync();

        return Results.Ok(essay);
    }

    [Authorize]
    [HttpGet("{id:int}/documentFile", Name = "DownloadEssayDocumentFile")]
    public async Task<IResult> DownloadFile(int id)
    {
        var essay = await context.Essays.FindAsync(id);
        if (essay == null) return Results.NotFound();
        var document = await context.Documents.FindAsync(essay.EdaiDocumentId);
        if (document == null) return Results.NotFound();
        var bytes = document.DocumentFile!;
        return Results.File(bytes, "application/octet-stream", document.DocumentName);
    }

    [Authorize]
    [HttpPut("{id:int}/indexedContent", Name = "AddIndexedContent")]
    public async Task<IResult> AddIndexedContent([FromRoute]int id ,IndexedContent indexedContent)
    {
        var essay = await context.Essays.FindAsync(id);
        if (essay == null) return Results.NotFound();
        essay.IndexedEssay!.Add(indexedContent);
        await context.SaveChangesAsync();
        return Results.Ok(essay.IndexedEssay);
    }
    
    [Authorize]
    [HttpDelete("{id:int}/indexedContent", Name = "DeleteIndexedContent")]
    public async Task<IResult> DeleteIndexedEssay([FromRoute]int id, int indexedContentId)
    {
        var essay = await context.Essays.Include(e => e.IndexedEssay).SingleOrDefaultAsync( e => e.EssayId == id);
        if (essay == null) return Results.NotFound();
        var indexedContent = essay.IndexedEssay.SingleOrDefault( idc => idc.IndexedContentId == indexedContentId);
        if (indexedContent == null) return Results.NotFound();
        essay.IndexedEssay!.Remove(indexedContent);
        context.IndexedContents.Remove(indexedContent);
        await context.SaveChangesAsync();
        return Results.Ok(indexedContentId);
    }
    
    [Authorize]
    [HttpGet("{id:int}/indexedEssay", Name = "GetIndexedEssay")]
    public async Task<IResult> GetIndexedEssay([FromRoute]int id)
    {
        var essay = await context.Essays.Include( e => e.IndexedEssay).ThenInclude( idc => idc.FeedbackComments).SingleOrDefaultAsync(e => e.EssayId == id);
        return essay == null ? Results.NotFound() : Results.Ok(essay.IndexedEssay);
    }

    [Authorize]
    [HttpPut("{id:int}/feedbackComment", Name = "AddFeedbackComment")]
    public async Task<IResult> AddFeedbackComment([FromRoute] int id, FeedbackComment feedbackComment)
    {
        var essay = await context.Essays.Include(e => e.IndexedEssay).SingleOrDefaultAsync( e => e.EssayId == id);
        if (essay == null) return Results.NotFound();
        //var indexedContents = essay.IndexedEssay.Where( idc => feedbackComment.RelatedText.Select( c => c.IndexedContentId).Contains() );
        //if (!indexedContents.Any() || indexedContents is null) return Results.NotFound();
        context.FeedbackComments.Add(feedbackComment);
        await context.SaveChangesAsync();
        return Results.Ok(feedbackComment);
    }
    
    [Authorize]
    [HttpGet("{id:int}/feedbackComment", Name = "GetFeedbackComment")]
    public async Task<IResult> GetFeedbackComments([FromRoute]int id)
    {
        var essay = await context.Essays
            .Include(e => e.IndexedEssay)
            .ThenInclude( idc => idc.FeedbackComments)
            .SingleOrDefaultAsync( e => e.EssayId == id);
        if (essay == null) return Results.NotFound();
        var indexedEssay = essay.IndexedEssay;
        var feedbackComments = new List<FeedbackComment>();
        foreach (var indexedContent in indexedEssay)
        {
            feedbackComments.AddRange(indexedContent.FeedbackComments);
        }
        return Results.Ok(feedbackComments);
    }
    
    [Authorize]
    [HttpDelete("{id:int}/feedbackComment", Name = "DeleteFeedbackComment")]
    public async Task<IResult> DeleteFeedbackComment([FromRoute]int id, int feedbackCommentId)
    {
        var essay = await context.Essays
            .Include(e => e.IndexedEssay)
            .ThenInclude( idc => idc.FeedbackComments)
            .SingleOrDefaultAsync( e => e.EssayId == id);
        if (essay == null) return Results.NotFound();
        var feedbackComment = await context.FeedbackComments.Include( fc => fc.RelatedTexts).SingleOrDefaultAsync( fc => fc.FeedbackCommentId == feedbackCommentId);
        if (feedbackComment == null) return Results.NotFound();
        var relatedText = feedbackComment.RelatedTexts;
        if (relatedText == null) return Results.NotFound();
        if (feedbackComment.RelatedTexts!.Count == 0) return Results.NotFound();
        //relatedText.FeedbackComments!.Remove(feedbackComment);
        context.FeedbackComments.Remove(feedbackComment);
        await context.SaveChangesAsync();
        return Results.Ok(feedbackCommentId);
    }
}