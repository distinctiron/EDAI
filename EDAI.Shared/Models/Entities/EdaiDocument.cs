namespace EDAI.Shared.Models.Entities;

public class EdaiDocument
{
    public int EdaiDocumentId { get; set; }
    
    public string DocumentName { get; set; }
    
    public string DocumentFileExtension { get; set; }
    
    public DateTime UploadDate { get; set; }
    
    public byte[] DocumentFile { get; set; }
}