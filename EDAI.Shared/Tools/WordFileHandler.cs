using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EDAI.Shared.Models;
using Color = DocumentFormat.OpenXml.Wordprocessing.Color;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Comment = DocumentFormat.OpenXml.Wordprocessing.Comment;

namespace EDAI.Shared.Tools;

public class WordFileHandler
{
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

    public async Task<IEnumerable<IndexedContent>> GetIndexedContent(Stream stream)
    {
        return await Task.Run(() =>
            {
                IEnumerable<IndexedContent> indexedContents;
                
                using (var wordDoc = WordprocessingDocument.Open(stream, false))
                {
                    indexedContents = IndexContent(wordDoc.MainDocumentPart.Document);
                }
                
                return indexedContents;
            }
        );
    }
    
    private IEnumerable<IndexedContent> IndexContent(Document document)
    {
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
                        Content = text.InnerText
                    };

                    textIndex++;
                }
                runIndex++;
            }
            paragraphIndex++;
        }
        
    }
    
    public async Task AddComments(Stream stream, EssayFeedback feedback)
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

            id = InsertComments(wordDoc, wordprocessingCommentsPart, feedback.GrammarComments, id);
            id = InsertComments(wordDoc, wordprocessingCommentsPart, feedback.ArgumentationComments, id);
            id = InsertComments(wordDoc, wordprocessingCommentsPart, feedback.EloquenceComments, id);

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

    public async Task FindText<T>(Stream stream, string searchText) where T : OpenXmlCompositeElement
    {
        stream.Position = 0;

        using (var wordDoc = WordprocessingDocument.Open(stream, false))
        {
            var texts = wordDoc.MainDocumentPart.Document.Body.Descendants<T>()
                .Where(t => t.InnerText.Contains(searchText));
            
        }
    }

    public async Task AddFeedback(Stream stream)
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
            var color = new Color() { Val = "FF0000" };
            styleRunProperties.Append(color);
            styleRunProperties.Append(italic);

            style.Append(styleRunProperties);

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
                
                var feedback = "Here is your feedback";
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
    
    public async Task AddFeedback(Stream stream, string feedback, System.Drawing.Color feedbackColor)
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
            var color = new Color() { Val = ColorToHex(feedbackColor) };
            styleRunProperties.Append(color);
            styleRunProperties.Append(italic);

            style.Append(styleRunProperties);

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

    private string ColorToHex(System.Drawing.Color color)
    {
        return $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }
    
}