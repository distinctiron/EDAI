using System.ComponentModel.DataAnnotations;
using EDAI.Shared.Models.Enums;

namespace EDAI.Shared.Models.DTO;

public class AssignmentDTO
{
    public int AssignmentId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }

    public IEnumerable<string> StudentClasses { get; set; }

    public bool Open { get; set; } = true;
    
    public int? ReferenceDocumentId { get; set; }

    public AssignmentType AssignmentType { get; set; }
}