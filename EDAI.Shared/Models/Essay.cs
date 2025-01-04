namespace EDAI.Shared.Models;

public class Essay
{
    public int EssayId { get; set; }
    public byte[]? File { get; set; }

    public ICollection<IndexedContent>? EssayAnswer { get; set; }
    
    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; } = null!;
    public int StudentId { get; set; }
    public Student? Student { get; set; } = null!;
    public int ScoreId { get; set; }
    public Score? Score { get; set; } = null!;
    
    public bool Evaluated { get; set; }
    
}