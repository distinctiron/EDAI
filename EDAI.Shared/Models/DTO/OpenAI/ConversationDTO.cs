using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models.DTO.OpenAI;

public class ConversationDTO
{
    public Score EssayScore { get; set; }

    public CommentsDTO Comments { get; set; }
}