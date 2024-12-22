using System.Text;
using DocumentFormat.OpenXml.Packaging;

namespace EDAI.Services;

public class WordFileReader : IFileReader
{
    public async Task<string> ReadFileAsync(Stream stream)
    {
        Console.WriteLine("Entered read method");
        
        return await Task.Run(() =>
            {
                StringBuilder text = new StringBuilder();
                
                Console.WriteLine("Created Stringbuilder");

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    Console.WriteLine("Opened Stream");
                    text.Append(body.InnerText);
                }
                
                return text.ToString();
            }
        );
    }
}