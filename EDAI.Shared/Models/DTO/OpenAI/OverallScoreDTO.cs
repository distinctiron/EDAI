namespace EDAI.Shared.Models.DTO.OpenAI;

public class OverallScoreDTO
{
    public ScoreDTO Score { get; set; }

    public FeedbackDTO Recommendation { get; set; }

    public FeedbackDTO Feedback { get; set; }
}