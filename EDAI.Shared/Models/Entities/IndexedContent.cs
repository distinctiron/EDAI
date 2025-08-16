using System.Text.Json.Serialization;

namespace EDAI.Shared.Models.Entities;

public class IndexedContent
{
    public int IndexedContentId { get; set; }
    public int ParagraphIndex { get; set; }

    public int RunIndex { get; set; }
    
    public int? FromCharInContent { get; set; }
    
    public int? ToCharInContent { get; set; }

    public string Content { get; set; }
    
    public int EssayId { get; set; }
    
    public Essay? Essay { get; set; } = null!;
    
    public ICollection<FeedbackComment>? FeedbackComments { get; set; } = new List<FeedbackComment>();
}