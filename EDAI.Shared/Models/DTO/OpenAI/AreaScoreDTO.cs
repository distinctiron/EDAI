namespace EDAI.Shared.Models.DTO.OpenAI;

public class AreaScoreDTO
{
    public ScoreDTO Score { get; set; }

    public CommentsDTO Comments { get; set; }

    public FeedbackDTO Recommendation { get; set; }
}