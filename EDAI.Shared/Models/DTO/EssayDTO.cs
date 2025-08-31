using EDAI.Shared.Models.DTO.OpenAI;

namespace EDAI.Shared.Models.DTO;

public class EssayDTO
{
    public int EssayId { get; set; }
    
    public int EdaiDocumentId { get; set; } 
    
    public int AssignmentId { get; set; }
    
    public string AssignmentName { get; set; } = null!;
    
    public int StudentId { get; set; }
    
    public string StudentName { get; set; }
    
    public ScoreDTO LatestScore { get; set; }
    
    public bool Evaluated { get; set; }
}