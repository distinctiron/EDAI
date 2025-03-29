using EDAI.Shared.Tools;

namespace EDAI.Shared.Factories;

public interface IWordFileHandlerFactory
{
    public WordFileHandler CreateWordFileHandler(Stream stream, int essayId);
}