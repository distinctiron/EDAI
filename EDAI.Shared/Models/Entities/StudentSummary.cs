using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models.DTO.OpenAI;

public class StudentSummary
{
    public int StudentId { get; set; }

    public Student? Student { get; set; }
    
    public string Summary { get; set; }

    public string FocusArea1 { get; set; }
    
    public string FocusArea2 { get; set; }
    
    public string FocusArea3 { get; set; }
}