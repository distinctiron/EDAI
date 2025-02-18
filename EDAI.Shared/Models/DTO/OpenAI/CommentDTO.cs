using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models.DTO.OpenAI;

public class CommentDTO
{
        public IndexedContentDTO RelatedText { get; set; }
        
        public string CommentFeedback { get; set; }
}