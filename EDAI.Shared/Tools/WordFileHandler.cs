using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EDAI.Shared.Models;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using Color = DocumentFormat.OpenXml.Wordprocessing.Color;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Comment = DocumentFormat.OpenXml.Wordprocessing.Comment;

namespace EDAI.Shared.Tools;

public class WordFileHandler : IWordFileHandler
{
    
    // This class needs a big refactor as methods are too long, contain too many parameters and have non-obvious side effects.
    // This will be done once UI is functional to ensure lower lead times for functional refactoring
    public async Task<string> ReadFileAsync(Stream stream)
    {
        return await Task.Run(() =>
            {
                StringBuilder text = new StringBuilder();

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    
                    text.Append(body.InnerText);
                }
                
                return text.ToString();
            }
        );
    }

    public async Task<IEnumerable<IndexedContent>> GetIndexedContent(Stream stream, int essayId)
    {
        return await Task.Run(() =>
            {
                IEnumerable<IndexedContent> indexedContents;
                
                using (var wordDoc = WordprocessingDocument.Open(stream, false))
                {
                    indexedContents = IndexContent(wordDoc.MainDocumentPart.Document, essayId);
                }
                
                return indexedContents;
            }
        );
    }

    public Dictionary<IndexedContent, Run> GetDictionaryContent(Stream stream, int essayId)
    {
        Dictionary<IndexedContent, Run> returnValue;

        using (var wordDoc = WordprocessingDocument.Open(stream, false))
        {
            returnValue = IndexDictionaryContent(wordDoc.MainDocumentPart.Document, essayId);
        }

        return returnValue;
    }

    public Dictionary<IndexedContent, Run> IndexDictionaryContent(Document document, int essayId)
    {

        var retrunvalue = new Dictionary<IndexedContent, Run>(); 
        
        var charCount = 0;
        
        var paragraphIndex = 0;
                    
        foreach (var paragraph in document.Descendants<Paragraph>())
        {
            var runIndex = 0;
                        
            foreach (var run in paragraph.Descendants<Run>())
            {
                var textIndex = 0;

                foreach (var text in run.Descendants<Text>())
                {
                    var content = new IndexedContent()
                    {
                        ParagraphIndex = paragraphIndex,
                        RunIndex = runIndex,
                        TextIndex = textIndex,
                        Content = text.InnerText,
                        FromCharInContent = ++charCount,
                        ToCharInContent = text.InnerText.Length + charCount,
                        EssayId = essayId
                    };
                    
                    retrunvalue.Add(content,run);

                    charCount += text.InnerText.Length;
                    textIndex++;
                }
                runIndex++;
            }
            paragraphIndex++;
        }

        return retrunvalue;
    }
    
    private IEnumerable<IndexedContent> IndexContent(Document document, int essayId)
    {
        var charCount = 0;
        
        var paragraphIndex = 0;
                    
        foreach (var paragraph in document.Descendants<Paragraph>())
        {
            var runIndex = 0;
                        
            foreach (var run in paragraph.Descendants<Run>())
            {
                var textIndex = 0;

                foreach (var text in run.Descendants<Text>())
                {
                    yield return new IndexedContent()
                    {
                        ParagraphIndex = paragraphIndex,
                        RunIndex = runIndex,
                        TextIndex = textIndex,
                        Content = text.InnerText,
                        FromCharInContent = ++charCount,
                        ToCharInContent = text.InnerText.Length + charCount,
                        EssayId = essayId
                    };

                    charCount += text.InnerText.Length;
                    textIndex++;
                }
                runIndex++;
            }
            paragraphIndex++;
        }
        
    }

    public async Task<EdaiDocument> CreateReviewDocument(int essayId, EdaiDocument essayAnswer, IEnumerable<CommentDTO> aiComments)
    {
        var reviewedStream = new MemoryStream();
        
        using (MemoryStream answerStream = new MemoryStream(essayAnswer.DocumentFile))
        {
            var dictionaryContent = GetDictionaryContent(answerStream, essayId);

            await answerStream.CopyToAsync(reviewedStream);

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(reviewedStream, true))
            {
                var runs = wordDoc.MainDocumentPart.Document.Descendants<Text>();
                
                foreach (var comment in aiComments)
                {
                    var startChar = comment.RelatedText.FromChar;
                    var endChar = comment.RelatedText.ToChar;

                    var startContent =
                        dictionaryContent.Keys.Single(i => i.FromCharInContent <= startChar && startChar < i.ToCharInContent);
            
                    var endContent = dictionaryContent.Keys.Single(i => i.FromCharInContent < endChar && endChar <= i.ToCharInContent);
                    
                    
                    //Scenario Start matches
                    if (startContent.FromCharInContent == startChar)
                    {
                        if (endContent.ToCharInContent == endChar)
                        {
                            var commentRun = dictionaryContent[endContent];
                            AddComments(reviewedStream, commentRun, comment.CommentFeedback);
                        }
                        else
                        {
                            var commentRun = SplitEndRun(reviewedStream, endContent, endChar);
                            AddComments(reviewedStream, commentRun, comment.CommentFeedback);
                        }
                        
                        
                        
                    }
                    else
                    {
                        var commentStartRun = SplitStartRun(reviewedStream, startContent, startChar);

                        if (endContent.ToCharInContent == endChar)
                        {
                            AddComments(reviewedStream, commentStartRun, comment.CommentFeedback);
                        }
                        else
                        {
                            var commentRun = SplitEndRun(reviewedStream, endContent, endChar);
                            AddComments(reviewedStream, commentRun, comment.CommentFeedback);
                        }
                    }
                    
                }
                
            }
            
        }

        
        
        
        return new EdaiDocument();
    }

    private IEnumerable<Run> handleCommentStart(IndexedContent startContent, CommentDTO comment)
    {
        if (startContent.FromCharInContent > comment.RelatedText.FromChar)
        {
            
        }
            
            
    }

    private bool isContentIdentical(IndexedContent content1, IndexedContent content2)
    {
        var isIdentical = content1.EssayId == content2.EssayId &&
                          content1.FromCharInContent == content2.FromCharInContent &&
                          content1.ToCharInContent == content2.ToCharInContent;
        
        return isIdentical;
    }

    public void AddComments(Stream stream, IndexedContent relatedContent, string comment)
    {
        
    }
    
    public async Task AddComments(Stream stream, IEnumerable<FeedbackComment> comments)
    {
        stream.Position = 0;
        
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream,true))
        {
            if (wordDoc is null)
            {
                throw new ArgumentNullException("Document is null");
            }

            WordprocessingCommentsPart wordprocessingCommentsPart =
                wordDoc.MainDocumentPart.WordprocessingCommentsPart ??
                wordDoc.MainDocumentPart.AddNewPart<WordprocessingCommentsPart>();

            //Paragraph firstParagraph = wordDoc.MainDocumentPart.Document.Descendants<Paragraph>().First();
            wordprocessingCommentsPart.Comments ??= new Comments();

            int id = 0;

            if (wordDoc.MainDocumentPart.GetPartsOfType<WordprocessingCommentsPart>().Any())
            {
                if (wordprocessingCommentsPart.Comments.HasChildren)
                {
                    id = (wordprocessingCommentsPart.Comments.Descendants<Comment>().Select(c =>
                    {
                        if (c.Id is not null && c.Id.Value is not null)
                        {
                            return int.Parse(c.Id.Value);
                        }
                        else
                        {
                            throw new ArgumentNullException("Comment id is null");
                        }
                    }).Max() + 1);
                }
            }

            id = InsertComments(wordDoc, wordprocessingCommentsPart, comments, id);

        }
        
    }

    private int InsertComments(WordprocessingDocument wordDoc, WordprocessingCommentsPart commentsPart,
        IEnumerable<FeedbackComment> feedbackComments, int currentHighestCommentId)
    {
        foreach (var feedbackComment in feedbackComments)
        {
            var commentContent = new Paragraph(new Run(new Text(feedbackComment.CommentFeedback)));

            var comment = new Comment()
            {
                Id = currentHighestCommentId.ToString(),
                Author = "EDAI",
                Initials = "EDAI",
                Date = DateTime.Now
            };

            comment.AppendChild(commentContent);
            commentsPart.Comments.AppendChild(comment);
            
            commentsPart.Comments.Save();

            var paragraph = wordDoc.MainDocumentPart.Document.Descendants<Paragraph>()
                .ElementAt(feedbackComment.RelatedText.ParagraphIndex);
            var run = paragraph.Descendants<Run>().ElementAt(feedbackComment.RelatedText.RunIndex);

            run.InsertBefore(new CommentRangeStart() {Id = currentHighestCommentId.ToString() }, run.GetFirstChild<Text>());

            var commentEnd = run.InsertAfter(new CommentRangeEnd() {Id = currentHighestCommentId.ToString() }, run.Elements<Text>().Last());

            run.InsertAfter(new Run(new CommentReference() { Id = currentHighestCommentId.ToString() }), commentEnd);

            currentHighestCommentId++;
        }

        return currentHighestCommentId;
    }
    
    public async Task AddFeedback(Stream stream, string feedback)
    {
        stream.Position = 0;
        
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, true))
        {
            if (wordDoc is null)
            {
                throw new ArgumentNullException("Document is null");
            }

            MainDocumentPart mainDocumentPart = wordDoc.MainDocumentPart ?? wordDoc.AddMainDocumentPart();
            mainDocumentPart.Document ??= new Document();
            mainDocumentPart.Document.Body ??= mainDocumentPart.Document.AppendChild(new Body());
            var body = wordDoc.MainDocumentPart!.Document!.Body;

            var part = wordDoc.MainDocumentPart.StyleDefinitionsPart;

            if (part is null)
            {
                part = wordDoc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
                var root = new Styles();
                root.Save(part);
            }

            var style = GetNewStyle(System.Drawing.Color.Aqua);

            if (part.Styles is not null)
            {
                part.Styles.Append(style);
            }
            else
            {
                part.Styles = new Styles();
                part.Styles.Append(style);
            }
            
            if (body != null)
            {
                var paragraph = new Paragraph();
                var run = new Run();

                paragraph.AppendChild(run);
                
                run.AppendChild(new Text(feedback));

                if (!paragraph.Elements<ParagraphProperties>().Any())
                {
                    paragraph.PrependChild<ParagraphProperties>(new ParagraphProperties());
                }

                if (paragraph.ParagraphProperties.ParagraphStyleId is null)
                {
                    paragraph.ParagraphProperties.ParagraphStyleId = new ParagraphStyleId();
                }

                paragraph.ParagraphProperties.ParagraphStyleId.Val = "FeedbackStyle";
                
                body.AppendChild(paragraph);
            }
        }
    }

    private Style GetNewStyle(System.Drawing.Color styleColor)
    {
        var style = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = "FeedbackStyle",
                CustomStyle = true,
                Default = false
            };
                
            Aliases aliases1 = new Aliases() { Val = "Alias" };
            AutoRedefine autoredefine1 = new AutoRedefine() { Val = OnOffOnlyValues.Off };
            BasedOn basedon1 = new BasedOn() { Val = "Normal" };
            LinkedStyle linkedStyle1 = new LinkedStyle() { Val = "OverdueAmountChar" };
            Locked locked1 = new Locked() { Val = OnOffOnlyValues.Off };
            PrimaryStyle primarystyle1 = new PrimaryStyle() { Val = OnOffOnlyValues.On };
            StyleHidden stylehidden1 = new StyleHidden() { Val = OnOffOnlyValues.Off };
            SemiHidden semihidden1 = new SemiHidden() { Val = OnOffOnlyValues.Off };
            StyleName styleName1 = new StyleName() { Val = "Feedback" };
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = "Normal" };
            UIPriority uipriority1 = new UIPriority() { Val = 1 };
            UnhideWhenUsed unhidewhenused1 = new UnhideWhenUsed() { Val = OnOffOnlyValues.On };
                
            style.Append(aliases1);
            style.Append(autoredefine1);
            style.Append(basedon1);
            style.Append(linkedStyle1);
            style.Append(locked1);
            style.Append(primarystyle1);
            style.Append(stylehidden1);
            style.Append(semihidden1);
            style.Append(styleName1);
            style.Append(nextParagraphStyle1);
            style.Append(uipriority1);
            style.Append(unhidewhenused1);

            var styleRunProperties = new StyleRunProperties();
            var italic = new Italic();
            var color = new Color() { Val = ColorToHex(styleColor) };
            styleRunProperties.Append(color);
            styleRunProperties.Append(italic);

            style.Append(styleRunProperties);

            return style;
    }
    
    private string ColorToHex(System.Drawing.Color color)
    {
        return $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }
    
}