using EDAI.Shared.Tools;

namespace EDAI.Shared.Factories;

public class WordFileHandlerFactory : IWordFileHandlerFactory
{
    public WordFileHandler CreateWordFileHandler(Stream stream, int essayId)
    {
        return new WordFileHandler(stream, essayId);
    }
}