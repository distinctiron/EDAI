namespace EDAI.Shared.Models.DTO.OpenAI;

public class DocumentDTO
{
    public string DocumentName { get; set; }
    
    public string DocumentFileExtension { get; set; }
    
    public byte[] DocumentFile { get; set; }
}