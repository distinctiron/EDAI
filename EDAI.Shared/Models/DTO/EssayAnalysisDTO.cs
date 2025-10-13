using EDAI.Shared.Models.DTO.OpenAI;
using ScoreDTO = EDAI.Shared.Models.Entities.ScoreDTO;

namespace EDAI.Shared.Models.DTO;

public class EssayAnalysisDTO
{
    public int EssayId { get; set; }

    public string EssayTitle { get; set; }
    
    public string AssignmentName { get; set; }

    public ScoreDTO Score { get; set; }
    
    public IEnumerable<FeedbackCommentDTO> Comments { get; set; }
}