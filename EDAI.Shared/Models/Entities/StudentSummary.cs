using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models.Entities;

public class StudentSummary
{
    public int StudentSummaryId { get; set; }
    
    public int StudentId { get; set; }

    public Student? Student { get; set; } = null!;
    
    public string Summary { get; set; }

    public string FocusArea1 { get; set; }
    
    public string FocusArea2 { get; set; }
    
    public string FocusArea3 { get; set; }
}