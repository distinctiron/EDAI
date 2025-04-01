using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.DTO.OpenAI;

namespace EDAI.Shared.Tools;

public static class CommentMapper
{
    public static CommentIndexedDTO assignCharPositions(string essayContent, CommentDTO aiComment)
    {
        var fromChar = essayContent.IndexOf(aiComment.RelatedText);
        var toChar = fromChar + aiComment.RelatedText.Length;
        return new CommentIndexedDTO(aiComment, fromChar,toChar);
    }
    public static IEnumerable<CommentIndexedDTO> assignCharPositions(string essayContent, IEnumerable<CommentDTO> aiComments)
    {
        foreach (var aiComment in aiComments)
        {
            yield return assignCharPositions(essayContent, aiComment);
        }
    }
}