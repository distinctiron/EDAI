namespace EDAI.Shared.Models;

public class Score
{
    public int ScoreId { get; set; }
    public float OverallScore { get; set; }
    public float ScoreTheme1 { get; set; }
    public float ScoreTheme2 { get; set; }
    public float ScoreThemeN { get; set; }
    public ICollection<Essay>? Essays { get; } = new List<Essay>();
}