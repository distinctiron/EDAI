using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models.DTO.OpenAI;

public class CommentDTO
{
        public CommentRelatedText RelatedText { get; set; }
        
        public string CommentFeedback { get; set; }
}