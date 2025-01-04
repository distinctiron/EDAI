namespace EDAI.Shared.Models;

public class IndexedContent
{
    public int IndexedContentId { get; set; }
    public int ParagraphIndex { get; set; }

    public int RunIndex { get; set; }

    public int TextIndex { get; set; }
    
    public int? FromCharInContent { get; set; }
    
    public int? ToCharInContent { get; set; }

    public string Content { get; set; }
}