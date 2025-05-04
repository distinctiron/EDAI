using EDAI.Shared.Models.Enums;

namespace EDAI.Shared.Models.Entities;

public class FeedbackComment : BaseComment
{
    public FeedbackComment() {}

    public FeedbackComment(BaseComment comment, CommentType commentType)
    {
        this.CommentType = commentType;
        this.CommentFeedback = comment.CommentFeedback;
        this.RelatedTexts = comment.RelatedTexts;
        this.FeedbackCommentId = comment.FeedbackCommentId;
    }
    
    public CommentType CommentType { get; set; }
}