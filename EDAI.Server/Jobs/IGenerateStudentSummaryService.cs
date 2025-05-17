namespace EDAI.Server.Jobs;

public interface IGenerateStudentSummaryService
{
    public Task GenerateStudentSummaryScore(int studentId, string connectionString);
}