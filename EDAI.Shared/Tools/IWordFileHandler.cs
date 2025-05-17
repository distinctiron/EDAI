using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Tools;

public interface IWordFileHandler
{
    public Task<IEnumerable<IndexedContent>> GetIndexedContent(Stream stream, int essayId);
    
    public Task AddFeedback(Stream stream, string feedback);

    public Task AddComments(Stream stream, IEnumerable<FeedbackComment> comments);
}