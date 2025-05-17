using System.ComponentModel.DataAnnotations;

namespace EDAI.Shared.Models.Entities;

public class Student
{
    public int StudentId { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Class { get; set; }

    public int StudentClassId { get; set; }

    public StudentClass? StudentClass { get; set; } = null!;

    public ICollection<StudentSummary>? StudentSummaries { get; set; } = new List<StudentSummary>();
    
    public int GraduationYear { get; set; }

    public ICollection<Essay>? Essays { get; } = new List<Essay>();
}