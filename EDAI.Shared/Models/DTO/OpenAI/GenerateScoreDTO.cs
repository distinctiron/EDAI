using EDAI.Shared.Models.DTO.OpenAI;

namespace EDAI.Shared.Models.Entities;

public class GenerateScoreDTO
{
    public float OverallScore { get; set; }
    
    public float ArgumentationScore { get; set; }

    public IEnumerable<CommentDTO> ArgumentationComments { get; set; }

    public string ArgumentationRecommendation { get; set; }
    
    public float GrammarScore { get; set; }
    
    public IEnumerable<CommentDTO> GrammarComments { get; set; }

    public string GrammarRecommendation { get; set; }
    
    public float EloquenceScore { get; set; }
    
    public IEnumerable<CommentDTO> EloquenceComments { get; set; }

    public string EloquenceRecommendation { get; set; }
    
    public string OverallStructure { get; set; }
    
    public float OverallStructureScore { get; set; }
    
    public string OverallStructureRecommendation { get; set; }
    
    public string AssignmentAnswer { get; set; }
    
    public float AssignmentAnswerScore { get; set; }
    
    public string AssignmentAnswerRecommendation { get; set; }
    
}