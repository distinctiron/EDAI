namespace EDAI.Shared.Models.Entities;

public class ScoreDTO
{
    public Student Student { get; set; }

    public string AssignmentName { get; set; }
    
    public int ScoreId { get; set; }
    
    public int EssayId { get; set; }
    public Essay? Essay { get; set; }
    public float OverallScore { get; set; }
    
    public float ArgumentationScore { get; set; }

    public string ArgumentationRecommendation { get; set; }
    public float GrammarScore { get; set; }

    public string GrammarRecommendation { get; set; }
    public float EloquenceScore { get; set; }

    public string EloquenceRecommendation { get; set; }
    public int? EvaluatedEssayDocumentId { get; set; }
    public EdaiDocument? EvaluatedEssayDocument { get; set; } 
    public string OverallStructure { get; set; }
    public float OverallStructureScore { get; set; }
    public string OverallStructureRecommendation { get; set; }
    public string AssignmentAnswer { get; set; }
    public float AssignmentAnswerScore { get; set; }
    public string AssignmentAnswerRecommendation { get; set; }
    
}