namespace EDAI.Shared.Models;

public class Essay
{
    public int EssayId { get; set; }
    public Assignment Assignment { get; set; }
    public Student Student { get; set; }
    public Score Score { get; set; }
}