using EDAI.Shared.Models.Enums;

namespace EDAI.Shared.Models.DTO.OpenAI;

public class FeedbackCommentDTO : CommentDTO
{
    public CommentType CommentType { get; set; }
}