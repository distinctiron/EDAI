using EDAI.Shared.Models.DTO.OpenAI;

namespace EDAI.Shared.Models.DTO;

public class CommentIndexedDTO : CommentDTO
{
    public CommentIndexedDTO(CommentDTO commentDto, int fromChar, int toChar)
    {
        CommentFeedback = commentDto.CommentFeedback;
        RelatedText = commentDto.RelatedText;
        FromChar = fromChar;
        ToChar = toChar;
    }
    
    public int FromChar { get; set; }
        
    public int ToChar { get; set; }
    
    
}