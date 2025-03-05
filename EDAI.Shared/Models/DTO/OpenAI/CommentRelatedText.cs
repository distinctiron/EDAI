namespace EDAI.Shared.Models.DTO.OpenAI;

public class CommentRelatedText
{
    public int FromChar { get; set; }
    
    public int ToChar { get; set; }

    public string Content { get; set; }
}