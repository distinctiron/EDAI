namespace EDAI.Shared.Models.Entities;

public class FeedbackComment
{
    public int FeedbackCommentId { get; set; }
    
    public int RelatedTextId { get; set; }
    public IndexedContent? RelatedText { get; set; } = null!;
    
    public string CommentFeedback { get; set; }
}