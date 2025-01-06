using System.ComponentModel.DataAnnotations;

namespace EDAI.Shared.Models.DTO;

public class AssignmentDTO
{
    public int AssignmentId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }

    public bool Open { get; set; } = true;
    
    public int? ReferenceDocumentId { get; set; }
    
    public string DocumentName { get; set; }
    
    public string DocumentFileExtension { get; set; }
    
    public DateTime UploadDate { get; set; }
    
    public byte[] DocumentFile { get; set; }
}