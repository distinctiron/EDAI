namespace EDAI.Shared.Models.Entities;

public class BaseComment
{
    public int FeedbackCommentId { get; set; }
    
    public int RelatedTextId { get; set; }
    
    public IndexedContent? RelatedText { get; set; } = null!;
    
    public string CommentFeedback { get; set; }
}