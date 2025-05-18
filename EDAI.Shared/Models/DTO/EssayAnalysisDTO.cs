using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models.DTO;

public class EssayAnalysisDTO
{
    public int EssayId { get; set; }

    public string EssayTitle { get; set; }
    
    public string AssignmentName { get; set; }

    public Score Score { get; set; }
    
    public IEnumerable<FeedbackComment> Comments { get; set; }
}