namespace EDAI.Shared.Models.Entities;

public class Essay
{
    public int EssayId { get; set; }
    
    public int EdaiDocumentId { get; set; } 
    public EdaiDocument? Document { get; set; }

    public ICollection<IndexedContent>? IndexedEssay { get; set; } = new List<IndexedContent>();
    
    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; } = null!;
    public int StudentId { get; set; }
    public Student? Student { get; set; } = null!;
    
    public ICollection<Score>? Scores { get; set; } = new List<Score>();
    
    public bool Evaluated { get; set; }
    
}