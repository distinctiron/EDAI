namespace EDAI.Shared.Models.DTO;

public class ClassAnalysisDTO
{
    public int ClassId { get; set; }
    
    public IEnumerable<StudentAnalysisDTO> StudentAnalysisDtos { get; set; }
}