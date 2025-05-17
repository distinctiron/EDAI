using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models.DTO;

public class StudentAnalysisDTO
{
    public int StudentId { get; set; }

    public IEnumerable<EssayAnalysisDTO> EssayAnalysese { get; set; }
}