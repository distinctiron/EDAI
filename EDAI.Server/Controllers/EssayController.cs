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
public class EssayController(EdaiContext context, IMapper _mapper) : ControllerBase
{
    [HttpGet(Name = "GetEssays")]
    public IEnumerable<Essay> GetEssays()
    {
        return context.Essays
            .Include(e => e.Assignment)
            .Include(e => e.Student)
            .Include(e => e.IndexedEssay)
            .Include(e => e.Scores);
    }

    [HttpGet("{id:int}", Name = "GetEssaysById")]
    public Essay? GetById(int id)
    {
        var essay = context.Essays
            .Include(e => e.Assignment)
            .Include(e => e.Student)
            .Include(e => e.Scores)
            .Include(e => e.IndexedEssay)
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

    [HttpPost("bulk", Name = "BulkAddEssay")]
    public ActionResult<List<int>> BulkAddEssay(IEnumerable<EssayFileDTO> essays)
    {
        
        var entities = _mapper.Map<IEnumerable<Essay>>(essays);
        foreach (var entity in entities)
        {
            entity.StudentId = entity.Student.StudentId;
            entity.Student = null;
        }
        context.Essays.AddRange(entities);
        context.SaveChanges();

        foreach (var essay in essays)
        {
            essay.EssayId = entities.Single(e => e.AssignmentId == essay.AssignmentId && 
                                                 e.StudentId == essay.Student.StudentId).EssayId;
        }

        return Ok(essays);
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
    
    [HttpPost("{id:int}/uploadDocumentFile", Name = "UploadEssayDocumentFile")]
    public IResult UploadFile([FromRoute]int id, IFormFile file)
    {
        var essay = context.Essays.Find(id);
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
        
        context.SaveChanges();

        return Results.Ok(essay);
    }

    [HttpGet("{id:int}/documentFile", Name = "DownloadEssayDocumentFile")]
    public IResult DownloadFile(int id)
    {
        var essay = context.Essays.Find(id);
        if (essay == null) return Results.NotFound();
        var document = context.Documents.Find(essay.EdaiDocumentId);
        if (document == null) return Results.NotFound();
        var bytes = document.DocumentFile!;
        return Results.File(bytes, "application/octet-stream", document.DocumentName);
    }

    [HttpPut("{id:int}/indexedContent", Name = "AddIndexedContent")]
    public IResult AddIndexedContent([FromRoute]int id ,IndexedContent indexedContent)
    {
        var essay = context.Essays.Find(id);
        if (essay == null) return Results.NotFound();
        essay.IndexedEssay!.Add(indexedContent);
        context.SaveChanges();
        return Results.Ok(essay.IndexedEssay);
    }
    
    [HttpDelete("{id:int}/indexedContent", Name = "DeleteIndexedContent")]
    public IResult DeleteIndexedEssay([FromRoute]int id, int indexedContentId)
    {
        var essay = context.Essays.Include(e => e.IndexedEssay).SingleOrDefault( e => e.EssayId == id);
        if (essay == null) return Results.NotFound();
        var indexedContent = essay.IndexedEssay.SingleOrDefault( idc => idc.IndexedContentId == indexedContentId);
        if (indexedContent == null) return Results.NotFound();
        essay.IndexedEssay!.Remove(indexedContent);
        context.IndexedContents.Remove(indexedContent);
        context.SaveChanges();
        return Results.Ok(indexedContentId);
    }
    
    [HttpGet("{id:int}/indexedEssay", Name = "GetIndexedEssay")]
    public IResult GetIndexedEssay([FromRoute]int id)
    {
        var essay = context.Essays.Include( e => e.IndexedEssay).ThenInclude( idc => idc.FeedbackComments).SingleOrDefault(e => e.EssayId == id);
        return essay == null ? Results.NotFound() : Results.Ok(essay.IndexedEssay);
    }

    [HttpPut("{id:int}/feedbackComment", Name = "AddFeedbackComment")]
    public IResult AddFeedbackComment([FromRoute] int id, FeedbackComment feedbackComment)
    {
        var essay = context.Essays.Include(e => e.IndexedEssay).SingleOrDefault( e => e.EssayId == id);
        if (essay == null) return Results.NotFound();
        //var indexedContents = essay.IndexedEssay.Where( idc => feedbackComment.RelatedText.Select( c => c.IndexedContentId).Contains() );
        //if (!indexedContents.Any() || indexedContents is null) return Results.NotFound();
        context.FeedbackComments.Add(feedbackComment);
        context.SaveChanges();
        return Results.Ok(feedbackComment);
    }
    
    [HttpGet("{id:int}/feedbackComment", Name = "GetFeedbackComment")]
    public IResult GetFeedbackComments([FromRoute]int id)
    {
        var essay = context.Essays
            .Include(e => e.IndexedEssay)
            .ThenInclude( idc => idc.FeedbackComments)
            .SingleOrDefault( e => e.EssayId == id);
        if (essay == null) return Results.NotFound();
        var indexedEssay = essay.IndexedEssay;
        var feedbackComments = new List<FeedbackComment>();
        foreach (var indexedContent in indexedEssay)
        {
            feedbackComments.AddRange(indexedContent.FeedbackComments);
        }
        return Results.Ok(feedbackComments);
    }
    
    [HttpDelete("{id:int}/feedbackComment", Name = "DeleteFeedbackComment")]
    public IResult DeleteFeedbackComment([FromRoute]int id, int feedbackCommentId)
    {
        var essay = context.Essays
            .Include(e => e.IndexedEssay)
            .ThenInclude( idc => idc.FeedbackComments)
            .SingleOrDefault( e => e.EssayId == id);
        if (essay == null) return Results.NotFound();
        var feedbackComment = context.FeedbackComments.Include( fc => fc.RelatedTexts).SingleOrDefault( fc => fc.FeedbackCommentId == feedbackCommentId);
        if (feedbackComment == null) return Results.NotFound();
        var relatedText = feedbackComment.RelatedTexts;
        if (relatedText == null) return Results.NotFound();
        if (feedbackComment.RelatedTexts!.Count == 0) return Results.NotFound();
        //relatedText.FeedbackComments!.Remove(feedbackComment);
        context.FeedbackComments.Remove(feedbackComment);
        context.SaveChanges();
        return Results.Ok(feedbackCommentId);
    }
}