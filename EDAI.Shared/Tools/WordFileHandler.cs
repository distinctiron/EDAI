using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Word.DrawingCanvas;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EDAI.Shared.Models;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using Color = DocumentFormat.OpenXml.Wordprocessing.Color;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Comment = DocumentFormat.OpenXml.Wordprocessing.Comment;
using CommentRangeEnd = DocumentFormat.OpenXml.Wordprocessing.CommentRangeEnd;
using ParagraphProperties = DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace EDAI.Shared.Tools;

public class WordFileHandler : IDisposable
{
    
    // This class needs a big refactor as methods are too long, contain too many parameters and have non-obvious side effects.
    // This will be done once UI is functional to ensure lower lead times for functional refactoring
    private Stream _answerDocumentStream;

    private Stream _reviewedDocumentStream;
    
    private WordprocessingDocument _answerDocument;
    
    private WordprocessingDocument _reviewedDocument;

    private bool _isOriginalDisposed = false;

    private bool _isReviewedDisposed = false;

    private int _essayId;

    public WordFileHandler(Stream stream, int essayId)
    {
        _answerDocumentStream = stream;
        _reviewedDocumentStream = stream;
        _essayId = essayId;

        _answerDocument = WordprocessingDocument.Open(_answerDocumentStream, false);
        _reviewedDocument = WordprocessingDocument.Open(_reviewedDocumentStream, false);
    }
    
    public void Dispose()
    {
        if (!_isOriginalDisposed)
        {
            _answerDocumentStream.Dispose();
            _isOriginalDisposed = true;
        }

        if (!_isReviewedDisposed)
        {
            _reviewedDocumentStream.Dispose();
            _isReviewedDisposed = true;
        }
        
        _reviewedDocument.Dispose();
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
                yield return new IndexedContent()
                {
                    EssayId = essayId,
                    ParagraphIndex = paragraphIndex,
                    RunIndex = runIndex,
                    Content = run.InnerText,
                    FromCharInContent = ++charCount,
                    ToCharInContent = run.InnerText.Length + charCount
                };

                charCount += run.InnerText.Length;
                
                runIndex++;
            }
            paragraphIndex++;
        }
        
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
                var content = new IndexedContent()
                {
                    ParagraphIndex = paragraphIndex,
                    RunIndex = runIndex,
                    Content = run.InnerText,
                    FromCharInContent = ++charCount,
                    ToCharInContent = run.InnerText.Length + charCount,
                    EssayId = essayId
                };
                    
                retrunvalue.Add(content,run);

                charCount += run.InnerText.Length;
                runIndex++;
            }
            paragraphIndex++;
        }

        return retrunvalue;
    }

    public async Task<(EdaiDocument, IEnumerable<IndexedContent>)> CreateReviewDocument(EdaiDocument essayAnswer, IEnumerable<CommentDTO> aiComments)
    {

        var dictionaryContent = IndexDictionaryContent(_reviewedDocument.MainDocumentPart.Document, _essayId);

        var runs = _reviewedDocument.MainDocumentPart.Document.Descendants<Run>();
        
        EnsureCommentsPartExists();

        foreach (var comment in aiComments)
        {
            var startChar = comment.RelatedText.FromChar;
            var endChar = comment.RelatedText.ToChar;
            
            var startContent =
                dictionaryContent.Keys.Single(i => i.FromCharInContent <= startChar && startChar < i.ToCharInContent);
            var startRun = dictionaryContent[startContent];
            
            var endContent = dictionaryContent.Keys.Single(i => i.FromCharInContent < endChar && endChar <= i.ToCharInContent);
            var endRun = dictionaryContent[endContent];
                    
            if (startContent.FromCharInContent == startChar)
            {
                if (startContent.ToCharInContent == endChar)
                {
                    AddComment(startRun, startRun, comment.CommentFeedback);
                }
                else if (endContent.ToCharInContent == endChar)
                { 
                    AddComment(startRun, endRun, comment.CommentFeedback);
                }
                else
                {
                    var splitEndRuns = SplitRun(endRun, endChar);
                    AddComment(startRun, splitEndRuns.Item1, comment.CommentFeedback);
                }
            }
            else
            {
                var splitStartRuns = SplitRun(startRun, startChar);
                var newStartRun = splitStartRuns.Item2;

                if (startContent.ToCharInContent == endChar)
                {
                    AddComment(newStartRun, newStartRun, comment.CommentFeedback);
                }
                else if (endContent.ToCharInContent == endChar)
                {
                    AddComment(newStartRun, endRun, comment.CommentFeedback);
                }
                else
                {
                    var endRunSplit = SplitRun(endRun, endChar);
                    AddComment(newStartRun, endRunSplit.Item1, comment.CommentFeedback);
                }
            }
        }

        var indexedContents = IndexContent(_reviewedDocument.MainDocumentPart.Document, _essayId);

        return (new EdaiDocument
        {
            DocumentFile = getArrayFromStream(_reviewedDocumentStream),
            DocumentName = essayAnswer.DocumentName + "Reviewed"
        }, indexedContents);
    }

    private byte[] getArrayFromStream(Stream stream)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

    public void AddComment(Run startRun, Run endRun, string comment)
    {
        var id = getNextCommentId();
        
        CreateComment(comment, id);
        
        startRun.InsertBefore(new CommentRangeStart()
        {
            Id = id
        }, startRun.Elements<Text>().First());

        var commentEnd = new CommentRangeEnd()
        {
            Id = id
        };
        
        endRun.InsertAfter(commentEnd, endRun.Elements<Text>().Last());
        
        endRun.InsertAfter(new Run(new CommentReference()
        {
            Id = id
        }), commentEnd);
    }

    private void CreateComment(string comment, string id)
    {
        var wordComment = new Comment
        {
            Id = id,
            Author = "EDAI",
            Initials = "EDAI",
            Date = DateTime.Now
        };
        
        var commentContent = new Paragraph(new Run(new Text(comment)));

        wordComment.Append(commentContent);

        var wordComments = _reviewedDocument.MainDocumentPart.WordprocessingCommentsPart.Comments;
        wordComments.Append(wordComment);
        wordComments.Save();
    }

    public (Run,Run) SplitRun(Run originalRun, int splitIndex)
    {
        if (originalRun is null || splitIndex <= 0)
            throw new ArgumentException($"Invalid arguments: originalRun = {originalRun}, splitIndex = {splitIndex}");
        
        var text = originalRun.InnerText;
        
        string firstRunContent = text.Substring(0, splitIndex);
        string secondRunContent = text.Substring(splitIndex);

        var firstRun = new Run(new Text(firstRunContent));
        var secondRun = new Run(new Text(secondRunContent));

        if (originalRun.RunProperties is not null)
        {
            firstRun.RunProperties = originalRun.RunProperties;
            secondRun.RunProperties = originalRun.RunProperties;
        }

        var runParent = originalRun.Parent;

        if (runParent is null)
        {
            throw new NullReferenceException($"Run with text: \"{originalRun.InnerText}\" does not have parent");
        }
        
        runParent.InsertBefore(firstRun, originalRun);
        runParent.InsertBefore(secondRun, originalRun);
        originalRun.Remove();

        return (firstRun, secondRun);

    }

    private void EnsureCommentsPartExists()
    {
        var wordProcessingCommentsPart = _reviewedDocument.MainDocumentPart.WordprocessingCommentsPart ??
                                         _reviewedDocument.MainDocumentPart.AddNewPart<WordprocessingCommentsPart>();

        wordProcessingCommentsPart.Comments ??= new Comments();
    }

    private string getNextCommentId()
    {
        
        var wordCommentParts = _reviewedDocument.MainDocumentPart.GetPartsOfType<WordprocessingCommentsPart>();

        if (!wordCommentParts.Any())
            return "0";
        
        WordprocessingCommentsPart wordCommentPart;

        try
        {
            wordCommentPart = wordCommentParts.Single();
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine("Document is corrupted as it contains more than one WordProcessingCommentsPart");
            Console.WriteLine(e);
            throw;
        }

        if (!wordCommentPart.Comments.HasChildren)
            return "0";

        var comments = wordCommentPart.Comments.Descendants<Comment>();
            
        var nextId = comments.Select(c =>
        {
            if (c.Id is not null && c.Id.Value is not null)
            {
                return int.Parse(c.Id.Value);
            }
            else
            {
                throw new ArgumentNullException("Comment id is null");
            }
        }).Max() + 1;

        return nextId.ToString();

    }
    
    public async Task AddComment(Stream stream, IEnumerable<FeedbackComment> comments)
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

            InsertComments(wordDoc, wordprocessingCommentsPart, comments, id);

        }
        
    }

    private void InsertComments(WordprocessingDocument wordDoc, WordprocessingCommentsPart commentsPart,
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

        //return currentHighestCommentId;
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