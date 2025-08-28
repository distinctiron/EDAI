using System.ComponentModel.DataAnnotations;
using EDAI.Shared.Models.Enums;

namespace EDAI.Shared.Models.Entities;

public class Assignment
{
    public int AssignmentId { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }

    public bool Open { get; set; } = true;

    public AssignmentType AssignmentType { get; set; } = AssignmentType.HTX;
    

    public ICollection<Essay>? Essays { get; } = new List<Essay>();
}