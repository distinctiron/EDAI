namespace EDAI.Shared.Models;

public class Score
{
    public int ScoreId { get; set; }

    public int EssayFeedbackId { get; set; }
    
    public EssayFeedback EssayFeedback { get; set; }
    
    public float OverallScore { get; set; }
    public float GrammarScore { get; set; }
    public float EloquenceScore { get; set; }
    public float AssignmentAnswerScore { get; set; }
    public Essay Answer { get; } = new Essay();
}