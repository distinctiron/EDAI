namespace EDAI.Shared.Models.DTO;

public class StudentAnalysisDTO
{
    public int StudentId { get; set; }

    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public string StudentClass { get; set; }
    public IEnumerable<EssayAnalysisDTO> EssayAnalysese { get; set; }
    
}