using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models.DTO;

public class EssayFileDTO
{
    public int AssignmentId { get; set; }

    public Student Student { get; set; }

    
    public EdaiDocument Document { get; set; }
    
}