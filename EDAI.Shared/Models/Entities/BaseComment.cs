namespace EDAI.Shared.Models.Entities;

public class BaseComment
{
    public int FeedbackCommentId { get; set; }
    
    public ICollection<IndexedContent>? RelatedTexts { get; set; } = new List<IndexedContent>();
    
    public string CommentFeedback { get; set; }
}