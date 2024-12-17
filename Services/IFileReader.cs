namespace EDAI.Services;

public interface IFileReader
{
    public Task<string> ReadFileAsync(Stream filestream);
}