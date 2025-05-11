using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using EDAI.Shared.Models.DTO;
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
    private Stream _reviewedDocumentStream;
    
    private WordprocessingDocument _reviewedDocument;

    private string _documentText;

    private bool _isReviewedDisposed = false;

    private int _essayId;

    private Dictionary<IndexedContent, Run> _dictionaryContent;

    private IEnumerable<IndexedContent> _indexedContents;

    public WordFileHandler(Stream stream, int essayId)
    {
        _reviewedDocumentStream = new MemoryStream();
        stream.Position = 0;
        stream.CopyTo(_reviewedDocumentStream);
        _reviewedDocumentStream.Position = 0;
        
        _reviewedDocument = WordprocessingDocument.Open(_reviewedDocumentStream, true);
        _documentText = _reviewedDocument.MainDocumentPart.Document.InnerText;
        
        _essayId = essayId;
        
        _dictionaryContent = IndexDictionaryContent(_reviewedDocument.MainDocumentPart.Document, _essayId);
        _indexedContents = IndexContent(_reviewedDocument.MainDocumentPart.Document, essayId);
    }
    
    public void Dispose()
    {
        if (!_isReviewedDisposed)
        {
            _reviewedDocumentStream.Dispose();
            _isReviewedDisposed = true;
        }
        
    }

    public string GetDocumentText()
    {
        return _documentText;
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
                    FromCharInContent = charCount,
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

        var returnvalue = new Dictionary<IndexedContent, Run>(); 
        
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
                    FromCharInContent = charCount,
                    ToCharInContent = run.InnerText.Length + charCount,
                    EssayId = essayId
                };
                    
                returnvalue.Add(content,run);

                charCount += run.InnerText.Length;
                runIndex++;
            }
            paragraphIndex++;
        }

        return returnvalue;
    }

    public async Task<(EdaiDocument, IEnumerable<IndexedContent>)> CreateReviewDocument(EdaiDocument essayAnswer, GenerateScoreDTO generatedScore)
    {
        
        EnsureCommentsPartExists();

        var aiComments = CommentMapper.assignCharPositions(_documentText ,generatedScore.ArgumentationComments.Concat(generatedScore.EloquenceComments)
            .Concat(generatedScore.GrammarComments));
        
        InsertComments(aiComments);
        
        InsertFeedback(generatedScore.OverallStructure, System.Drawing.Color.Chocolate);
        InsertFeedback(generatedScore.AssignmentAnswer, System.Drawing.Color.Firebrick);
        
        var reviewedName = string.Concat(essayAnswer.DocumentName,"Reviewed");

        var validator = new OpenXmlValidator();

        foreach (var error in validator.Validate(_reviewedDocument))
        {
            Console.WriteLine($"Error Id: {error.Id}, Error Type: {error.ErrorType}, Description: {error.Description}");
        }
        
        _reviewedDocument.MainDocumentPart.Document.Save();
        _reviewedDocument.MainDocumentPart.WordprocessingCommentsPart.Comments.Save();
        _reviewedDocument.Dispose(); // This is done here in order to flush changes to the document into stream
        
        return (new EdaiDocument
        {
            DocumentFile = getArrayFromStream(_reviewedDocumentStream),
            DocumentFileExtension = ".docx",
            DocumentName = reviewedName
        }, _indexedContents);
    }

    private void InsertComments(IEnumerable<CommentIndexedDTO> aiComments)
    {
        foreach (var comment in aiComments)
        {
            try
            {

                Console.WriteLine($"Processing comment with related text: {comment.RelatedText}");
            
                var startChar = comment.FromChar;
                var endChar = comment.ToChar;
            
                Console.WriteLine($"startchar: {startChar}");
                Console.WriteLine($"startchar: {endChar}");
            
            
                var startContent =
                    _dictionaryContent.Keys.Single(i => i.FromCharInContent <= startChar && startChar < i.ToCharInContent);
                var startRun = _dictionaryContent[startContent];
            
                var endContent = _dictionaryContent.Keys.Single(i => i.FromCharInContent < endChar && endChar <= i.ToCharInContent);
                var endRun = _dictionaryContent[endContent];

                var areStartEndIdentical = startRun == endRun;
                    
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
                        if (areStartEndIdentical)
                        {
                            AddComment(splitEndRuns.Item1, splitEndRuns.Item1, comment.CommentFeedback);
                        }
                        else
                        {
                            AddComment(startRun, splitEndRuns.Item1, comment.CommentFeedback);   
                        }
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
                        if (areStartEndIdentical)
                        {
                            var endRunSplit = SplitRun(splitStartRuns.Item2, endChar);
                            AddComment(endRunSplit.Item1, endRunSplit.Item1, comment.CommentFeedback);
                        }
                        else
                        {
                            var endRunSplit = SplitRun(endRun, endChar);
                            AddComment(newStartRun, endRunSplit.Item1, comment.CommentFeedback);   
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }
            
            UpdateDocumentState();
            
        }
    }

    private void UpdateDocumentState()
    {
        _indexedContents = IndexContent(_reviewedDocument.MainDocumentPart.Document, _essayId);
        _dictionaryContent = IndexDictionaryContent(_reviewedDocument.MainDocumentPart.Document, _essayId);
    }

    private byte[] getArrayFromStream(Stream stream)
    {
        stream.Position = 0;
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
        
        
        startRun.Parent?.InsertBefore(new CommentRangeStart()
        {
            Id = id
        }, startRun);

        var commentEnd = new CommentRangeEnd()
        {
            Id = id
        };
        
        endRun?.Parent.InsertAfter(commentEnd, endRun);
        
        endRun?.Parent.InsertAfter(new Run(new CommentReference()
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
        var startIndex = _documentText.IndexOf(text, StringComparison.Ordinal);
        var runSplitIndex = splitIndex - startIndex;

        try
        {
            string firstRunContent = text.Substring(0, runSplitIndex);
            string secondRunContent = text.Substring(runSplitIndex);
            var firstRun = new Run(new Text(firstRunContent));
            var secondRun = new Run(new Text(secondRunContent));

            if (originalRun.RunProperties is not null)
            {
                firstRun.RunProperties = CloneRunProperties(originalRun);
                secondRun.RunProperties = CloneRunProperties(originalRun);
            }

            var runParent = GetRunParent(originalRun);

            if (runParent is null)
            {
                throw new NullReferenceException($"Run with text: \"{originalRun.InnerText}\" does not have parent");
            }
        
            runParent.InsertBefore(firstRun, originalRun);
            runParent.InsertBefore(secondRun, originalRun);
            Console.WriteLine($"Run containing text \"{originalRun.InnerText}\" has been split");
            originalRun.Remove();
            UpdateDocumentState();

            return (firstRun, secondRun);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //throw;
        }

        return (new Run(), new Run());

    }

    private RunProperties? CloneRunProperties(Run run)
    {
        return run?.RunProperties is not null
            ? (RunProperties)run.RunProperties.CloneNode(true)
            : null;
    }

    private Paragraph GetRunParent(Run run)
    {
        var parent = run.Parent as Paragraph;

        if (parent is null)
        {
            try
            {
                parent = _reviewedDocument.MainDocumentPart.Document.Descendants<Run>()
                    .Single( r => r.InnerText == run.InnerText).Parent as Paragraph;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Cannot find run with innertext \"{run.InnerText}\"");
                throw;
            }
        }

        return parent;
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

    private void InsertFeedback(string feedback, System.Drawing.Color feedbackColor)
    {
        var stylesPart = _reviewedDocument.MainDocumentPart.StyleDefinitionsPart;
        if (stylesPart is null)
        {
            stylesPart = _reviewedDocument.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            var root = new Styles();
            root.Save(stylesPart);
        }

        var styleId = Guid.NewGuid().ToString();

        var style = GetNewStyle(feedbackColor, styleId);

        if (stylesPart.Styles is not null)
        {
            stylesPart.Styles.Append(style);
        }
        else
        {
            stylesPart.Styles = new Styles();
            stylesPart.Styles.Append(style);
        }
        
        var body = _reviewedDocument.MainDocumentPart.Document.Body;

        if (body is null)
            throw new InvalidOperationException("Document has no body");

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

        paragraph.ParagraphProperties.ParagraphStyleId.Val = styleId;
        
        var sectPr = body.Elements<SectionProperties>().FirstOrDefault();

        if (sectPr != null)
        {
            body.InsertBefore(paragraph, sectPr);
        }
        else
        {
            body.AppendChild(paragraph);
        }
        
        UpdateDocumentState();

    }
    

    private Style GetNewStyle(System.Drawing.Color styleColor, string id)
    {
        var style = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = id,
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
            StyleName styleName1 = new StyleName() { Val = id };
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = "Normal" };
            UIPriority uipriority1 = new UIPriority() { Val = 1 };
            UnhideWhenUsed unhidewhenused1 = new UnhideWhenUsed() { Val = OnOffOnlyValues.On };
                
            style.Append(styleName1);
            style.Append(basedon1);
            style.Append(nextParagraphStyle1);
            style.Append(linkedStyle1);
            style.Append(uipriority1);
            style.Append(primarystyle1);
            //style.Append(aliases1);
            //style.Append(autoredefine1);
            style.Append(locked1);
            //style.Append(stylehidden1);
            //style.Append(semihidden1);
            //style.Append(unhidewhenused1);

            var styleRunProperties = new StyleRunProperties();
            var runFonts = new RunFonts()
            {
                Ascii = "Times New Roman",
                HighAnsi = "Times New Roman",
                EastAsia = "Times New Roman",
                ComplexScript = "Times New Roman"
            };
            var italic = new Italic();
            var color = new Color() { Val = ColorToHex(styleColor) };
            styleRunProperties.Append(runFonts);
            styleRunProperties.Append(italic);
            styleRunProperties.Append(color);
            

            style.Append(styleRunProperties);

            return style;
    }
    
    private string ColorToHex(System.Drawing.Color color)
    {
        return $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }
    
}