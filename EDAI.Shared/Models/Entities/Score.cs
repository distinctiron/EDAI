namespace EDAI.Shared.Models.Entities;

public class Score
{
    public int ScoreId { get; set; }
    
    public int EssayId { get; set; }
    public Essay? Essay { get; set; }
    public float OverallScore { get; set; }
    public float GrammarScore { get; set; }
    public float EloquenceScore { get; set; }
    public float AssignmentAnswerScore { get; set; }
    public int? EvaluatedEssayDocumentId { get; set; }
    public EdaiDocument? EvaluatedEssayDocument { get; set; } 
    public string OverallStructure { get; set; }
    public string AssignmentAnswer { get; set; }
}