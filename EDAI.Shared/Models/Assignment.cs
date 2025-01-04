using System.ComponentModel.DataAnnotations;

namespace EDAI.Shared.Models;

public class Assignment
{
    public int AssignmentId { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }

    public bool Open { get; set; } = true;
    
    public byte[]? ReferenceTextFile { get; set; }

    public ICollection<Essay>? Essays { get; } = new List<Essay>();
}