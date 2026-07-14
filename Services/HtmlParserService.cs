using HtmlAgilityPack;
using MigraDoc.DocumentObjectModel;
using PickupAPi.Utils;

namespace PickupAPi.Services
{
    public class HtmlParserService
    {
        private readonly TableService _tables;
        private readonly ImageService _images;
        private readonly BlockquoteService _blockquotes;

        public HtmlParserService(TableService tables, ImageService images, BlockquoteService blockquotes)
        {
            _tables = tables;
            _images = images;
            _blockquotes = blockquotes;
        }

        public void ProcessHtmlContent(string htmlContent, Section section)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var article = htmlDoc.DocumentNode.SelectSingleNode("//article") ?? htmlDoc.DocumentNode;
            ProcessNode(article, section);
        }

        internal void ProcessNode(HtmlNode node, Section section, Paragraph paragraph = null)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    var text = HtmlUtils.Clean(child.InnerText);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (paragraph != null) paragraph.AddText(text);
                        else section.AddParagraph(text);
                    }
                }
                else if (child.NodeType == HtmlNodeType.Element)
                {
                    if (child.Name == "button" || child.Name == "i")
                        continue;

                    switch (child.Name.ToLower())
                    {
                        case "h1": AddHeading(section, child.InnerText, "Heading1"); break;
                        case "h2": AddHeading(section, child.InnerText, "Heading2"); break;
                        case "h3": AddHeading(section, child.InnerText, "Heading3"); break;
                        case "p":
                            var text = HtmlUtils.Clean(child.InnerText);
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                if (paragraph != null) paragraph.AddText(text);
                                else section.AddParagraph(text);
                            }
                            break;
                        case "blockquote": _blockquotes.Add(section, child); break;
                        case "ul": _tables.AddList(section, child, false); break;
                        case "ol": _tables.AddList(section, child, true); break;
                        case "table": _tables.Add(section, child, this); break;
                        case "img": _images.Add(section, child); break;
                        case "a":
                            //var href = child.GetAttributeValue("href", "#");
                            //var link = section.AddHyperlink(href, HyperlinkType.Web);
                            //link.AddFormattedText(HtmlUtils.Clean(child.InnerText), TextFormat.Underline);
                            break;
                        case "div":
                        case "article":
                        case "section":
                            ProcessNode(child, section);
                            break;
                    }
                }
            }
        }

        private void AddHeading(Section section, string text, string style)
        {
            var para = section.AddParagraph(HtmlUtils.Clean(text));
            para.Style = style;
        }
    }
}