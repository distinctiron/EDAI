using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;

namespace EDAI.Services.Interfaces;

public interface IOpenAiService
{
    public void SetIndexedContents(IEnumerable<IndexedContent> indexedContents, string assignmentDescription, string referencetext = null);
    public Task<IEnumerable<FeedbackComment>> GetCommentsAsync(CommentType commentType, string prompt);
    public Task<string> GetFeedbackAsync(string prompt);
    public Task<float> GetScoreAsync(string prompt);
}