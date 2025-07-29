using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;

namespace EDAI.Shared.Models.DTO;

public class EssayFileDTO
{
    public int? EssayId { get; set; }
    
    public int AssignmentId { get; set; }

    public StudentDTO Student { get; set; }
    
    public EdaiDocument? Document { get; set; }

    public int? EdaiDocumentId { get; set; }
    public EssayStatus Status { get; set; } = EssayStatus.AwaitingUpload;
}