namespace EDAI.Shared.Models;

public class EssayFeedback
{
    public IEnumerable<FeedbackComment> GrammarComments { get; set; }
    
    public IEnumerable<FeedbackComment> ArgumentationComments { get; set; }
    
    public IEnumerable<FeedbackComment> EloquenceComments { get; set; }

    public string OverallStructure { get; set; }

    public string AssignmentAnswer { get; set; }
}