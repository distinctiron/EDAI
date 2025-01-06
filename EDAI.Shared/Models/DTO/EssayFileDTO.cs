namespace EDAI.Shared.Models.DTO;

public class EssayFileDTO
{
    public int AssignmentId { get; set; }

    public int StudentId { get; set; }

    public byte[] DocumentFile { get; set; }

    public string DocumentName { get; set; }

    public string DocumentFileExtension { get; set; }
}