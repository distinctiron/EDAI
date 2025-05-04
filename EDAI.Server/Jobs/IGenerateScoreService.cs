namespace EDAI.Server.Jobs;

public interface IGenerateScoreService
{
    public Task GenerateScore(IEnumerable<int> documentIds);
}