using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace EDAI.Shared.Tools;

public static class PdfFileHandler
{
    public static async Task<string> ExtractTextFromPdf(byte[] pdfFile)
    {
        //var pdfPath = "/Users/sujeevanferrum/Repos/EDAI/Static/Reference.pdf";
        
        var text = new StringBuilder();
        
        try
        {
            var memoryStream = new MemoryStream(pdfFile);
            
            using (var pdfDoc = new PdfDocument(new PdfReader(memoryStream)))
            {
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var strategy = new LocationTextExtractionStrategy();
                    var currentText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                    text.Append(currentText);
                }
            }

            return text.ToString();
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}