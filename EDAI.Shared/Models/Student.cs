using System.ComponentModel.DataAnnotations;

namespace EDAI.Shared.Models;

public class Student
{
    public int StudentId { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Class { get; set; }
    [Required]
    public int GraduationYear { get; set; }

    public ICollection<Essay>? Essays { get; } = new List<Essay>();
}