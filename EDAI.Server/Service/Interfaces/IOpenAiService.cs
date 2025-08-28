using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;

namespace EDAI.Services.Interfaces;

public interface IOpenAiService
{
    public void InitiateScoreConversation(string essayText, string assignmentDescription, AssignmentType assignmentType,  IEnumerable<string> referenceTexts);
    public void InitiateStudentSummaryConversation();
    public Task<IEnumerable<FeedbackComment>> GetCommentsAsync(CommentType commentType, string prompt);
    public Task<string> GetFeedbackAsync(string prompt);
    public Task<float> GetScoreAsync(string prompt);
    public Task<GenerateScoreDTO> AssessEssayAsync();
    public Task<StudentSummaryDTO> GenerateStudentSummary(IEnumerable<Score> scores);
}