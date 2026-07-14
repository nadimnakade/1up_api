using HtmlAgilityPack;
using Microsoft.Ajax.Utilities;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using PickupAPi.Models;
using System.Text.RegularExpressions;


namespace PickupAPi.Controllers
{
    //[EnableCors(origins: "https://nomadix-kms.document360.io", headers: "*", methods: "*")]
    [RoutePrefix("api/BusinessPdf")]
    public class _BusinessPdfController : ApiController
    {

        int mainsrno = 1;
        int subsrno = 1;
        private readonly List<(string Title, int PageNumber)> blockquoteIndex = new List<(string, int)>();
        bool isFirst = true;
        [HttpPost]
        [Route("GeneratePdf")]
        [ResponseType(typeof(byte[]))]
        public async Task<HttpResponseMessage> GeneratePdf([FromBody] HtmlRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.htmlContent))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid HTML Content");

            try
            {

                // Debug: Log the received HTML content
                Console.WriteLine($"Received HTML content length: {request.htmlContent?.Length ?? 0}");
                Console.WriteLine($"HTML content preview: {request.htmlContent?.Substring(0, Math.Min(200, request.htmlContent?.Length ?? 0))}");

                string strHTMLContent = request.htmlContent;


                byte[] pdfBytes = GenerateBusinessPdf(request.htmlContent, request.CoverPageType);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(pdfBytes)
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "NomadixBusinessDocument.pdf"
                };

                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.StackTrace);
            }
        }

        public byte[] GenerateBusinessPdf(string htmlContent, int coverPageType = 0)
        {
            Document doc = new Document();

            // HtmlAgilityPack document
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            if (!string.IsNullOrEmpty(htmlContent))
            {
                htmlDoc.LoadHtml(htmlContent); // Load the HTML content
            }

            DefineStyles(doc);

            // Add cover page conditionally based on coverPageType
            //if (coverPageType == 1)
            //{
            //    AddCustomCoverPage(doc);
            //}
            //else
            //{
            //    AddCoverPage(doc);
            //}
            AddCoverPage(doc);
            // Add index page as the second page
            //AddIndexPage(doc, htmlDoc);

            // Add content section with header and footer
            Section contentSection = doc.AddSection();
            AddHeader(contentSection);
            AddFooter(contentSection);

            // Define page setup for content
            PageSetup pageSetup = contentSection.PageSetup;
            pageSetup.PageWidth = Unit.FromCentimeter(21);    // A4 width
            pageSetup.PageHeight = Unit.FromCentimeter(29.7); // A4 height
            pageSetup.TopMargin = Unit.FromCentimeter(2.5);   // Reserve space for header
            pageSetup.BottomMargin = Unit.FromCentimeter(2.5); // Reserve space for footer
            pageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            pageSetup.RightMargin = Unit.FromCentimeter(2.5);

            // Process HTML content
            ProcessHtmlContent(htmlContent, contentSection);

            // Render document to PDF
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = doc;
            renderer.RenderDocument();

            using (MemoryStream stream = new MemoryStream())
            {
                renderer.PdfDocument.Save(stream, false);
                return stream.ToArray();
            }
        }

        private List<(string Title, string Bookmark)> ExtractHeadings(HtmlDocument doc)
        {
            var headings = new List<(string, string)>();
            var headingNodes = doc.DocumentNode.SelectNodes("//h1|//h2|//h3");

            if (headingNodes != null)
            {
                foreach (var heading in headingNodes)
                {
                    string id = heading.GetAttributeValue("id", $"heading_{Guid.NewGuid()}");
                    headings.Add((heading.InnerText.Trim(), id));
                }
            }

            return headings;
        }

        private void AddIndexPage(Document doc, HtmlDocument htmlDoc)
        {
            // Validate input parameters
            if (doc == null || htmlDoc == null)
            {
                return; // Exit if either parameter is null
            }
            
            // Create a new section for the index page
            Section indexSection = doc.AddSection();
            
            // Set up page formatting
            indexSection.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21),    // A4 width
                PageHeight = Unit.FromCentimeter(29.7),  // A4 height
                TopMargin = Unit.FromCentimeter(2.5),
                BottomMargin = Unit.FromCentimeter(2.5),
                LeftMargin = Unit.FromCentimeter(2.5),
                RightMargin = Unit.FromCentimeter(2.5)
            };
            
            // Add header and footer to index page
            AddHeader(indexSection);
            AddFooter(indexSection);
            
            // Add title
            Paragraph title = indexSection.AddParagraph("Table of Contents");
            //title.Format.Font.Name = "Montserrat";
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = Unit.FromCentimeter(1);
            
            // Find all h3 elements (heading3 tags) to use as headers for the index
            var h3Elements = htmlDoc.DocumentNode?.SelectNodes("//h3");
            
            if (h3Elements != null && h3Elements.Any())
            {
                // Dictionary to track h2 numbering
                Dictionary<string, int> h2Numbering = new Dictionary<string, int>();
                int currentH2Number = 1;
                string currentH2 = null;
                
                // Create a table for the index entries
                Table indexTable = indexSection.AddTable();
                indexTable.Borders.Visible = false;
                indexTable.AddColumn(Unit.FromCentimeter(2));  // Number column
                indexTable.AddColumn(Unit.FromCentimeter(12)); // Title column
                indexTable.AddColumn(Unit.FromCentimeter(2));  // Page number column
                
                foreach (var h3Element in h3Elements)
                {
                    if (h3Element == null) continue;
                    
                    // Find the parent or preceding h2 element
                    HtmlNode parentNode = h3Element.ParentNode;
                    HtmlNode h2Element = null;
                    
                    // Look for the closest h2 element
                    HtmlNode previousNode = h3Element.PreviousSibling;
                    while (previousNode != null)
                    {
                        if (previousNode.Name == "h2")
                        {
                            h2Element = previousNode;
                            break;
                        }
                        previousNode = previousNode.PreviousSibling;
                    }
                    
                    // If no h2 found as previous sibling, look for h2 in ancestors
                    if (h2Element == null)
                    {
                        var ancestors = h3Element.Ancestors();
                        foreach (var ancestor in ancestors)
                        {
                            if (ancestor.Name == "h2")
                            {
                                h2Element = ancestor;
                                break;
                            }
                        }
                    }
                    
                    string h2Text = h2Element != null ? h2Element.InnerText?.Trim() ?? "" : "";
                    string h3Text = h3Element.InnerText?.Trim() ?? "";
                    
                    // If we have a new h2 section, add it to the index
                    if (!string.IsNullOrEmpty(h2Text) && (currentH2 == null || currentH2 != h2Text))
                    {
                        currentH2 = h2Text;
                        if (!h2Numbering.ContainsKey(h2Text))
                        {
                            h2Numbering[h2Text] = currentH2Number++;
                        }
                        
                        // Add h2 entry to index
                        Row h2Row = indexTable.AddRow();
                        h2Row.Cells[0].AddParagraph(h2Numbering[h2Text].ToString()).Format.Font.Bold = true;
                        h2Row.Cells[1].AddParagraph(h2Text).Format.Font.Bold = true;
                        
                        // Reset h3 counter for this h2
                        subsrno = 1;
                    }
                    
                    // Add h3 entry to index with proper numbering
                    if (currentH2 != null && h2Numbering.ContainsKey(currentH2))
                    {
                        Row h3Row = indexTable.AddRow();
                        h3Row.Cells[0].AddParagraph($"{h2Numbering[currentH2]}.{subsrno++}");
                        h3Row.Cells[1].AddParagraph(h3Text);
                    }
                }
            }
        }

        private void AddTableOfContents(Section section, List<(string Title, string Bookmark)> headings)
        {
            section.AddPageBreak();

            Paragraph title = section.AddParagraph("Table of Contents");
            title.Format.Font.Bold = true;
            title.Format.Font.Size = 16;
            title.Format.SpaceAfter = "1cm";

            Table tocTable = section.AddTable();
            tocTable.Borders.Visible = false;
            tocTable.AddColumn("14cm");
            tocTable.AddColumn("2cm");

            foreach (var heading in headings)
            {
                Row row = tocTable.AddRow();

                // Title with hyperlink
                Paragraph titlePara = row.Cells[0].AddParagraph();
                Hyperlink hyperlink = titlePara.AddHyperlink(heading.Bookmark);
                hyperlink.AddText(heading.Title);
                hyperlink.Font.Bold = true;

                // Page number
                Paragraph pagePara = row.Cells[1].AddParagraph();
                pagePara.AddPageRefField(heading.Bookmark);
                pagePara.Format.Alignment = ParagraphAlignment.Right;
            }
        }
        //private void AddTableOfContents(Section section)
        //{
        //    Paragraph title = section.AddParagraph("Table of Contents");
        //    title.Format.Font.Bold = true;
        //    title.Format.Font.Size = 14;
        //    title.Format.SpaceAfter = "10pt";

        //    foreach (var (headingText, index) in blockquoteIndex.Distinct())
        //    {
        //        Paragraph tocEntry = section.AddParagraph();
        //        tocEntry.Style = "Normal";

        //        tocEntry.AddHyperlink($"heading_{index}", HyperlinkType.Bookmark).AddText(headingText);
        //        tocEntry.AddTab();
        //        tocEntry.Format.TabStops.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
        //        tocEntry.AddPageRefField($"heading_{index}");
        //    }
        //}

        private void AddCoverPage(Document doc)
        {
            Section section = doc.AddSection();
            section.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21),    // A4 width
                PageHeight = Unit.FromCentimeter(29.7),  // A4 height
                TopMargin = Unit.FromCentimeter(0),
                BottomMargin = Unit.FromCentimeter(0),
                LeftMargin = Unit.FromCentimeter(0),
                RightMargin = Unit.FromCentimeter(0)
            };


            // Add header image (person with phone)
            var headerImage = section.AddImage(HttpContext.Current.Server.MapPath("~/logo/header-logo.png"));
            headerImage.Width = section.PageSetup.PageWidth;
            headerImage.Height = Unit.FromCentimeter(18);
            headerImage.RelativeVertical = RelativeVertical.Page;
            headerImage.RelativeHorizontal = RelativeHorizontal.Page;
            headerImage.Top = Unit.FromCentimeter(0);
            headerImage.Left = Unit.FromCentimeter(0);
            headerImage.WrapFormat.Style = WrapStyle.Through;


            var middleImage = section.AddImage(HttpContext.Current.Server.MapPath("~/logo/middle.png"));
            middleImage.Width = section.PageSetup.PageWidth;
            middleImage.Height = Unit.FromCentimeter(7);
            middleImage.RelativeVertical = RelativeVertical.Page;
            middleImage.RelativeHorizontal = RelativeHorizontal.Page;
            middleImage.Top = Unit.FromCentimeter(19);
            middleImage.Left = Unit.FromCentimeter(0);
            middleImage.WrapFormat.Style = WrapStyle.Through;


            // Create a table for the footer
            //section.Headers.Primary.AddImage(HttpContext.Current.Server.MapPath("~/logo/header-image.jpg"));
            Table footerTable = section.Footers.Primary.AddTable();
            footerTable.Borders.Width = 0;
            footerTable.AddColumn(Unit.FromCentimeter(16)); // Text column
            footerTable.AddColumn(Unit.FromCentimeter(5));  // Logo column


            Row footerRow = footerTable.AddRow();

            // Set the background color
            footerRow.Shading.Color = new Color(122, 181, 92); // Light green background

            // Add the left-aligned text
            Paragraph leftText = footerRow.Cells[0].AddParagraph();
            leftText.AddFormattedText("® NOMADIX", TextFormat.Bold);
            //leftText.Format.Font.Name = "Montserrat";
            leftText.Format.Font.Size = 11;
            leftText.Format.Font.Color = Colors.White;
            leftText.Format.Alignment = ParagraphAlignment.Left;
            leftText.Format.LeftIndent = Unit.FromCentimeter(0.5); // Adjust left indent

            // Set vertical alignment for the left cell
            footerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;



            // Adjust the height of the row to fit content
            footerRow.Height = Unit.FromCentimeter(1.4);



        }

        private void AddCustomCoverPage(Document doc)
        {
            Section section = doc.AddSection();
            section.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21),    // A4 width
                PageHeight = Unit.FromCentimeter(29.7),  // A4 height
                TopMargin = Unit.FromCentimeter(0),
                BottomMargin = Unit.FromCentimeter(0),
                LeftMargin = Unit.FromCentimeter(0),
                RightMargin = Unit.FromCentimeter(0)
            };

            // Add the attached image as full page cover
            var coverImage = section.AddImage(HttpContext.Current.Server.MapPath("~/logo/pdfbackground.jpg"));
            coverImage.Width = section.PageSetup.PageWidth;
            coverImage.Height = section.PageSetup.PageHeight;
            coverImage.RelativeVertical = RelativeVertical.Page;
            coverImage.RelativeHorizontal = RelativeHorizontal.Page;
            coverImage.Top = Unit.FromCentimeter(0);
            coverImage.Left = Unit.FromCentimeter(0);
            coverImage.WrapFormat.Style = WrapStyle.Through;
        }

        private void AddHeader(Section section)
        {
            HeaderFooter header = section.Headers.Primary;

            // Create a table for the header
            Table headerTable = header.AddTable();
            headerTable.Borders.Width = 0;
            headerTable.AddColumn(Unit.FromCentimeter(10)); // Title column
            headerTable.AddColumn(Unit.FromCentimeter(6)); // Logo column

            Row headerRow = headerTable.AddRow();

            // Add title to the left
            Paragraph titleParagraph = headerRow.Cells[0].AddParagraph("Administration Guide");
            //titleParagraph.Format.Font.Name = "Montserrat";
            titleParagraph.Format.Font.Size = 12;
            titleParagraph.Format.Font.Bold = true;
            titleParagraph.Format.Alignment = ParagraphAlignment.Left;
            headerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;

            // Add logo to the right
            // Use the existing logo file or create a text placeholder for TrustedWiFi
            Paragraph logoText = headerRow.Cells[1].AddParagraph("TrustedWiFi");
            //logoText.Format.Font.Name = "Montserrat";
            logoText.Format.Font.Size = 14;
            logoText.Format.Font.Bold = true;
            logoText.Format.Alignment = ParagraphAlignment.Right;
            headerRow.Cells[1].VerticalAlignment = VerticalAlignment.Center;

            // Add a horizontal line above the footer
            Paragraph line = header.AddParagraph();
            line.Format.Borders.Top.Width = 0.2;
            line.Format.Borders.Top.Color = Colors.Gray;
            line.Format.SpaceAfter = Unit.FromCentimeter(0.2);
        }

        private void AddFooter(Section section)
        {
            HeaderFooter footer = section.Footers.Primary;

            // Add 12pt margin between body and footer
            Paragraph spacer = footer.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromPoint(12);

            // Add a horizontal line above the footer
            Paragraph line = footer.AddParagraph();
            line.Format.Borders.Top.Width = 0.1;
            line.Format.Borders.Top.Color = Colors.LightGray;

            // Create a table for the footer
            Table footerTable = footer.AddTable();
            footerTable.Borders.Width = 0;
            footerTable.AddColumn(Unit.FromCentimeter(15)); // Main content column
            footerTable.AddColumn(Unit.FromCentimeter(1)); // Page number column

            Row footerRow = footerTable.AddRow();
            footerRow.Height = Unit.FromCentimeter(1);

            // Add year and confidentiality text in one line
            string currentMonthYear = DateTime.Now.ToString("MMMM yyyy");
            Paragraph mainParagraph = footerRow.Cells[0].AddParagraph();
            mainParagraph.Format.TabStops.AddTabStop("3cm");
            mainParagraph.AddText(currentMonthYear);
            mainParagraph.AddTab();
            mainParagraph.AddText("Information subject to change without notice");
            mainParagraph.Format.Font.Size = 8;
            mainParagraph.Format.Font.Color = Colors.Gray; // Lighter gray color
            mainParagraph.Format.Font.Bold = false; // Remove bold for lighter appearance
            mainParagraph.Format.Alignment = ParagraphAlignment.Left;
            footerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;

            // Add page number to the right
            Paragraph pageNumberParagraph = footerRow.Cells[1].AddParagraph();
            pageNumberParagraph.AddPageField();
            pageNumberParagraph.Format.Font.Size = 8;
            pageNumberParagraph.Format.Font.Color = Colors.Gray; // Lighter gray color
            pageNumberParagraph.Format.Font.Bold = false; // Remove bold for lighter appearance
            pageNumberParagraph.Format.Alignment = ParagraphAlignment.Right;
            footerRow.Cells[1].VerticalAlignment = VerticalAlignment.Center;
        }

        private List<int> headingNumbers = new List<int>();

        private string GetNumberedHeading(int level)
        {
            while (headingNumbers.Count < level)
                headingNumbers.Add(0);
            for (int i = level; i < headingNumbers.Count; i++)
                headingNumbers[i] = 0;
            headingNumbers[level - 1]++;
            return string.Join(".", headingNumbers.Take(level));
        }

        private void DefineStyles(Document doc)
        {
            // Normal text style
            Style normal = doc.Styles["Normal"];
            normal.Font.Name = "Arial";
            normal.Font.Size = 9;

            // Heading styles
            Style heading1 = doc.Styles["Heading1"];
            heading1.Font.Name = "Arial";
            heading1.Font.Size = 16;
            heading1.Font.Bold = true;
            heading1.Font.Color = new Color(24, 24, 27); // Nomadix blue color
            heading1.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.3);
            heading1.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.5);

            Style heading2 = doc.Styles["Heading2"];
            heading2.Font.Name = "Arial";
            heading2.Font.Size = 14;
            heading2.Font.Bold = true;
            heading2.Font.Color = new Color(24, 24, 27); // Nomadix blue color
            heading2.ParagraphFormat.SpaceBefore = Unit.FromPoint(8); // IS-003: 8pt before
            heading2.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);  // IS-003: 6pt after

            Style heading3 = doc.Styles["Heading3"];
            heading3.Font.Name = "Arial";
            heading3.Font.Size = 12;
            heading3.Font.Bold = true;
            heading3.Font.Color = new Color(24, 24, 27); // Nomadix blue color
            heading3.ParagraphFormat.SpaceBefore = Unit.FromPoint(12); // 12pt padding before subheadings
            heading3.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);   // 6pt after for clear separation

            // Create custom styles for notes and warnings
            Style noteStyle = doc.Styles.AddStyle("NoteBox", "Normal");
            noteStyle.ParagraphFormat.Borders.Width = 0.5;
            noteStyle.ParagraphFormat.Borders.Color = new Color(0, 106, 138); // Nomadix teal color
            noteStyle.ParagraphFormat.Borders.Distance = 3;
            noteStyle.ParagraphFormat.Shading.Color = new Color(28, 74, 113); // Nomadix blue background
            noteStyle.ParagraphFormat.LeftIndent = 9;
            noteStyle.ParagraphFormat.RightIndent = 9;
            noteStyle.Font.Color = Colors.White;
            noteStyle.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.6);
            noteStyle.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.4);

            Style warningStyle = doc.Styles.AddStyle("WarningBox", "Normal");
            warningStyle.ParagraphFormat.Borders.Width = 0.5;
            warningStyle.ParagraphFormat.Borders.Color = new Color(127, 100, 22); // Warning border color
            warningStyle.ParagraphFormat.Borders.Distance = 3;
            warningStyle.ParagraphFormat.Shading.Color = new Color(253, 242, 206); // Warning background color
            warningStyle.ParagraphFormat.LeftIndent = 9;
            warningStyle.ParagraphFormat.RightIndent = 9;
            warningStyle.Font.Color = new Color(127, 100, 22); // Warning text color
            warningStyle.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.6);
            warningStyle.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.4);

            // Table styles - standardized formatting
            Style tableStyle = doc.Styles.AddStyle("Table", "Normal");
            tableStyle.Font.Name = "Arial";
            tableStyle.Font.Size = 10; // Increased from 9pt to 10pt for better readability
            tableStyle.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(1.0); // Space before table
            tableStyle.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.8);  // Space after table
            tableStyle.ParagraphFormat.LineSpacing = Unit.FromPoint(12); // Reduced line spacing to prevent crowding

            Style tableHeader = doc.Styles.AddStyle("TableHeader", "Table");
            tableHeader.Font.Bold = true;
            tableHeader.Font.Name = "Arial";
            tableHeader.Font.Size = 10; // Consistent font size
            tableHeader.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            tableHeader.ParagraphFormat.Shading.Color = new Color(191, 191, 191); // Gray background for headers


        }

        private void ProcessHtmlContent(string htmlContent, Section section)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Process the root node (article)
            var articleNode = htmlDoc.DocumentNode.SelectSingleNode("//article");
            if (articleNode != null)
            {
                ProcessHtmlNode(articleNode, section);
            }
            else
            {
                // If no article tag, process the entire document
                ProcessHtmlNode(htmlDoc.DocumentNode, section);
            }
        }

        private void ProcessHtmlNode(HtmlNode node, Section section)
        {

            if (node == null) return;

            foreach (var childNode in node.ChildNodes)
            {
                // Skip elements with id="download-this-guide⬇"
                if (childNode.NodeType == HtmlNodeType.Element &&
                    childNode.GetAttributeValue("id", "") == "download-this-guide⬇")
                {
                    continue;
                }

                // Check for headings to add to table of contents
                //if (childNode.Name.StartsWith("h") && childNode.Name.Length == 2)
                //{
                //    int headingLevel;
                //    if (int.TryParse(childNode.Name.Substring(1), out headingLevel) && headingLevel <= 3)
                //    {
                //        if (!childNode.HasChildNodes || (childNode.ChildNodes.Count == 1 && childNode.FirstChild.Name != "p"))
                //        {
                //            string headingNumber = GetNumberedHeading(headingLevel);
                //            string title = $"{headingNumber} {childNode.InnerText.Trim()}";

                //            string bookmarkName = $"heading_{blockquoteIndex.Count + 1}";
                //            var headingPara = section.AddParagraph();
                //            headingPara.AddBookmark(bookmarkName);

                //            blockquoteIndex.Add((title, blockquoteIndex.Count + 1));

                //            var para = section.AddParagraph(title);
                //            para.Style = $"Heading{headingLevel}";
                //            para.Format.SpaceAfter = Unit.FromPoint(5);
                //        }
                //        return;
                //    }
                //}

                if (childNode.NodeType == HtmlNodeType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(childNode.InnerText))
                    {
                        Paragraph para = section.AddParagraph(childNode.InnerText.Trim());
                    }
                }
                else if (childNode.NodeType == HtmlNodeType.Element)
                {
                    switch (childNode.Name.ToLower())
                    {
                        case "h1":
                            AddHeading(section, childNode.InnerText, "Heading1");
                            break;
                        case "h2":
                            AddHeading(section, childNode.InnerText, "Heading2");
                            subsrno++;
                            break;
                        case "h3":
                            AddHeading(section, childNode.InnerText, "Heading3");
                            subsrno = 1;
                            mainsrno++;
                            break;
                        case "p":
                            AddParagraph(section, childNode);
                            break;
                        case "blockquote":
                            //AddBlockquote(section, childNode);
                            string type = "";
                            // Pseudocode:
                            // - The current condition uses IndexOf("Notes:") > 0, which will not match if "Notes:" is at the start (index 0).
                            // - To match "Notes:" anywhere (including at the start), use IndexOf("Notes:") >= 0.
                            // - Alternatively, use StartsWith("Notes:") if you only want to match when it is at the beginning.

                            if (childNode.InnerText.Trim().IndexOf("Notes") >= 0)
                            {
                                string str = "";
                                type = HttpContext.Current.Server.MapPath("~/logo/info_icon.png");
                                //AddSpace(section);
                                // Light blue background: #ddf7ff (221, 247, 255)
                                AddStyledSection(section, "Note", childNode.InnerText.Trim(), MigraDoc.DocumentObjectModel.Color.FromRgb(221, 247, 255), MigraDoc.DocumentObjectModel.Color.FromRgb(28, 74, 113), type);
                            }
                            else if (childNode.InnerText.Trim().IndexOf("Warning") >= 0)
                            {
                                type = HttpContext.Current.Server.MapPath("~/logo/warning_icon.png");
                                // Light yellow background: #fdf2ce (253, 242, 206), brown text: #7f6416 (127, 100, 22)
                                AddStyledSection(section, "Warning", childNode.InnerText.Trim(), MigraDoc.DocumentObjectModel.Color.FromRgb(253, 242, 206), MigraDoc.DocumentObjectModel.Color.FromRgb(127, 100, 22), type);
                            }
                            else if (childNode.InnerText.Trim().IndexOf("Tip") >= 0)
                            {
                                type = HttpContext.Current.Server.MapPath("~/logo/tip_icon.png");
                                // Green color matching the image: #8BC34A (139, 195, 74)
                                AddStyledSection(section, "Tip", childNode.InnerText.Trim(), MigraDoc.DocumentObjectModel.Color.FromRgb(139, 195, 74), Colors.White, type);
                            }
                            // Add the blockquote title to the index list
                            //blockquoteIndex.Add((childNode.InnerText.Trim(), 0)); // Page number will be updated later     
                            break;
                        case "ul":
                            AddList(section, childNode, false);
                            break;
                        case "ol":
                            AddList(section, childNode, true);
                            break;
                        case "table":
                            AddTable(section, childNode);
                            break;
                        case "hr":
                            AddHorizontalLine(section);
                            break;
                        case "img":
                            AddImage(section, childNode);
                            break;
                        case "div":
                        case "article":
                        case "section":
                            // Recursively process container elements
                            ProcessHtmlNode(childNode, section);
                            break;
                    }
                }
            }
        }

        private void AddStyledSection(Section section, string title, string content, Color backgroundColor, Color textColor, string iconPath = null)
        {
            // Add spacing before the styled section
            Paragraph spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            spacer.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            // Create a table to structure the section with rounded appearance
            Table table = section.AddTable();
            table.Borders.Width = 0;
            table.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            table.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            // Add columns: one for the icon, one for the text content
            table.AddColumn(Unit.FromCentimeter(1.8)); // Icon column (slightly wider)
            table.AddColumn(Unit.FromCentimeter(13.7)); // Text column

            // Add a row to the table
            Row row = table.AddRow();
            row.HeightRule = RowHeightRule.AtLeast;
            row.Height = Unit.FromCentimeter(1.2); // Minimum height that adjusts to content

            // Set the background color and padding
            row.Shading.Color = backgroundColor;
            row.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            row.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            // Add the icon cell
            Cell iconCell = row.Cells[0];
            iconCell.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            iconCell.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            iconCell.VerticalAlignment = VerticalAlignment.Center;
            iconCell.Format.Alignment = ParagraphAlignment.Center;

            // Add left border to the first cell only
            Color borderColor = Colors.Black; // Default
            if (title == "Note")
            {
                borderColor = MigraDoc.DocumentObjectModel.Color.FromRgb(28, 74, 113); // #1C4A71
            }
            else if (title == "Warning")
            {
                borderColor = MigraDoc.DocumentObjectModel.Color.FromRgb(127, 100, 22); // #7f6416
            }
            
            // Apply left border to the first cell only
            iconCell.Borders.Left.Width = Unit.FromPoint(4);
            iconCell.Borders.Left.Color = borderColor;

            // Add icon based on title type
            string defaultIconPath = GetDefaultIconPath(title);
            string finalIconPath = !string.IsNullOrEmpty(iconPath) ? iconPath : defaultIconPath;

            if (!string.IsNullOrEmpty(finalIconPath) && File.Exists(finalIconPath))
            {
                try
                {
                    Paragraph iconPara = iconCell.AddParagraph();
                    iconPara.Format.Alignment = ParagraphAlignment.Center;
                    var iconImage = iconPara.AddImage(finalIconPath);
                    iconImage.LockAspectRatio = true;
                    iconImage.Width = Unit.FromCentimeter(1.0);
                    iconImage.Height = Unit.FromCentimeter(1.0);
                    iconImage.Left = ShapePosition.Center;
                    iconImage.Top = ShapePosition.Center;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding icon image: {ex.Message}");
                    // Add text icon as fallback
                    AddTextIcon(iconCell, title, Colors.White);
                }
            }
            else
            {
                // Add text icon as fallback
                AddTextIcon(iconCell, title, Colors.White);
            }

            // Add the content cell
            Cell contentCell = row.Cells[1];
            contentCell.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            contentCell.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            contentCell.VerticalAlignment = VerticalAlignment.Top;
            contentCell.Format.LeftIndent = Unit.FromCentimeter(0.2);

            // Remove emojis if present
            string updatedContent = content.Replace("\u2139\uFE0F", "") // ℹ️
                                           .Replace("\u26A0\uFE0F", "") // ⚠️
                                           .Replace("\uD83D\uDCA1", ""); // 💡

            // Create title paragraph
            Paragraph titleParagraph = contentCell.AddParagraph();
            string displayTitle = GetDisplayTitle(title);
            titleParagraph.AddFormattedText(displayTitle, TextFormat.Bold);
            titleParagraph.Format.Font.Color = textColor;
            titleParagraph.Format.Font.Size = 11;
            titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            // Clean the main content
            string mainContent = CleanContent(updatedContent, title);

            // Create content paragraph
            Paragraph contentParagraph = contentCell.AddParagraph();

            // Handle content with lists
            if (mainContent.Contains("<li>") || mainContent.Contains("<ul>"))
            {
                ProcessListContent(contentCell, mainContent, textColor);
            }
            else
            {
                // Add plain text content
                contentParagraph.AddText(mainContent);
                contentParagraph.Format.Font.Color = textColor;
                contentParagraph.Format.Font.Size = 10;
                contentParagraph.Format.LineSpacing = Unit.FromCentimeter(0.4);
            }

            // Add spacing after section
            spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            spacer.Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }


        private void AddStyledSectionOld(Section section, string title, string content, Color backgroundColor, Color textColor, string iconPath = null)
        {
            // Add spacing before the styled section
            Paragraph spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            spacer.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            // Create a table to structure the section with rounded appearance
            Table table = section.AddTable();
            table.Borders.Width = 0;
            table.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            table.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            // Add columns: one for the icon, one for the text content
            table.AddColumn(Unit.FromCentimeter(1.8)); // Icon column (slightly wider)
            table.AddColumn(Unit.FromCentimeter(13.7)); // Text column

            // Add a row to the table
            Row row = table.AddRow();
            row.HeightRule = RowHeightRule.AtLeast;
            row.Height = Unit.FromCentimeter(1.2); // Minimum height that adjusts to content

            // Set the background color and padding
            row.Shading.Color = backgroundColor;
            row.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            row.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            // Add the icon cell
            Cell iconCell = row.Cells[0];
            iconCell.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            iconCell.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            iconCell.VerticalAlignment = VerticalAlignment.Center;
            iconCell.Format.Alignment = ParagraphAlignment.Center;

            // Add icon based on title type
            string defaultIconPath = GetDefaultIconPath(title);
            string finalIconPath = !string.IsNullOrEmpty(iconPath) ? iconPath : defaultIconPath;

            if (!string.IsNullOrEmpty(finalIconPath) && File.Exists(finalIconPath))
            {
                try
                {
                    Paragraph iconPara = iconCell.AddParagraph();
                    iconPara.Format.Alignment = ParagraphAlignment.Center;
                    var iconImage = iconPara.AddImage(finalIconPath);
                    iconImage.LockAspectRatio = true;
                    iconImage.Width = Unit.FromCentimeter(1.0);
                    iconImage.Height = Unit.FromCentimeter(1.0);
                    iconImage.Left = ShapePosition.Center;
                    iconImage.Top = ShapePosition.Center;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding icon image: {ex.Message}");
                    // Add text icon as fallback
                    AddTextIcon(iconCell, title, Colors.White);
                }
            }
            else
            {
                // Add text icon as fallback
                AddTextIcon(iconCell, title, Colors.White);
            }

            // Add the content cell
            Cell contentCell = row.Cells[1];
            contentCell.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            contentCell.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            contentCell.VerticalAlignment = VerticalAlignment.Top;
            contentCell.Format.LeftIndent = Unit.FromCentimeter(0.2);

            // Remove emojis if present
            string updatedContent = content.Replace("\u2139\uFE0F", "") // ℹ️
                                           .Replace("\u26A0\uFE0F", "") // ⚠️
                                           .Replace("\uD83D\uDCA1", ""); // 💡

            // Create title paragraph
            Paragraph titleParagraph = contentCell.AddParagraph();
            string displayTitle = GetDisplayTitle(title);
            titleParagraph.AddFormattedText(displayTitle, TextFormat.Bold);
            titleParagraph.Format.Font.Color = textColor;
            titleParagraph.Format.Font.Size = 11;
            titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            // Clean the main content
            string mainContent = CleanContent(updatedContent, title);

            // Create content paragraph
            Paragraph contentParagraph = contentCell.AddParagraph();

            // Handle content with lists
            if (mainContent.Contains("<li>") || mainContent.Contains("<ul>"))
            {
                ProcessListContent(contentCell, mainContent, textColor);
            }
            else
            {
                // Add plain text content
                contentParagraph.AddText(mainContent);
                contentParagraph.Format.Font.Color = textColor;
                contentParagraph.Format.Font.Size = 10;
                contentParagraph.Format.LineSpacing = Unit.FromCentimeter(0.4);
            }

            // Add spacing after section
            spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            spacer.Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }

        private string GetDefaultIconPath(string title)
        {
            switch (title.ToLower())
            {
                case "note":
                case "notes":
                    return HttpContext.Current.Server.MapPath("~/logo/info_icon.png");
                case "warning":
                    return HttpContext.Current.Server.MapPath("~/logo/warning_icon.png");
                case "tip":
                    return HttpContext.Current.Server.MapPath("~/logo/tip_icon.png");
                default:
                    return null;
            }
        }

        private void AddTextIcon(Cell iconCell, string title, Color textColor)
        {
            Paragraph iconParagraph = iconCell.AddParagraph();
            string iconText = GetIconText(title);
            iconParagraph.AddFormattedText(iconText, TextFormat.Bold);
            iconParagraph.Format.Font.Color = textColor; // Use text color instead of white
            iconParagraph.Format.Font.Size = 22;
            iconParagraph.Format.Alignment = ParagraphAlignment.Center;
            iconParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            iconParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);
        }

        private string GetIconText(string title)
        {
            switch (title.ToLower())
            {
                case "note":
                case "notes":
                    return "ℹ";
                case "warning":
                    return "⚠";
                case "tip":
                    return "💡";
                default:
                    return "ℹ";
            }
        }

        private string GetDisplayTitle(string title)
        {
            switch (title.ToLower())
            {
                case "note":
                    return "Notes";
                case "warning":
                    return "Warning";
                case "tip":
                    return "Tip:";
                default:
                    return title;
            }
        }

        private string CleanContent(string content, string title)
        {
            string mainContent = content;
            
            // Remove title words from content
            switch (title.ToLower())
            {
                case "note":
                case "notes":
                    mainContent = mainContent.Replace("Notes:", "").Replace("Note:", "").Replace("Note", "").Trim();
                    break;
                case "warning":
                    mainContent = mainContent.Replace("Warning:", "").Replace("Warning", "").Trim();
                    break;
                case "tip":
                    mainContent = mainContent.Replace("Tip:", "").Replace("Tip", "").Trim();
                    break;
            }
            
            return mainContent;
        }

        private void ProcessListContent(Cell contentCell, string mainContent, Color textColor)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(mainContent);

            // Normal text before lists
            var textNodes = htmlDoc.DocumentNode.ChildNodes
                .Where(n => n.NodeType == HtmlNodeType.Text)
                .Select(n => n.InnerText.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t));

            foreach (var text in textNodes)
            {
                Paragraph textPara = contentCell.AddParagraph();
                textPara.AddText(text);
                textPara.Format.Font.Color = textColor;
                textPara.Format.Font.Size = 10;
                textPara.Format.SpaceAfter = Unit.FromCentimeter(0.1);
            }

            // Lists
            var lists = htmlDoc.DocumentNode.SelectNodes("//ul");
            if (lists != null)
            {
                foreach (var list in lists)
                {
                    var items = list.SelectNodes("./li");
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            Paragraph listItemPara = contentCell.AddParagraph();
                            listItemPara.Format.LeftIndent = Unit.FromCentimeter(0.3);
                            listItemPara.AddText("• " + WebUtility.HtmlDecode(item.InnerText.Trim()));
                            listItemPara.Format.Font.Color = textColor;
                            listItemPara.Format.Font.Size = 10;
                            listItemPara.Format.SpaceBefore = Unit.FromCentimeter(0.05);
                            listItemPara.Format.SpaceAfter = Unit.FromCentimeter(0.05);
                        }
                    }
                }
            }
        }



        private void AddHeading(Section section, string text, string style, int fontSize = 16)
        {
            string strContext = "";
            if (string.IsNullOrWhiteSpace(text)) return;
            // else if (style == "Heading3")
            // {
            //     strContext = mainsrno.ToString() + " . ";
                
            // }
            // else if (style == "Heading2")
            // {
            //     strContext = mainsrno.ToString() + " . " + subsrno.ToString();
            // }

            Paragraph heading = section.AddParagraph();
            heading.Style = style;
            heading.Format.Font.Size = isFirst ? 20 : fontSize;
            heading.Format.Font.Bold = true;
            heading.Format.Font.Color = Color.FromRgb(81, 162, 198); ;  
            // Clean up the text (remove HTML tags and decode entities)
            string cleanText = WebUtility.HtmlDecode(StripHtml(text)).Trim();
            //heading.AddText(strContext + " " + cleanText);
            heading.AddText(cleanText);

            isFirst = false;
        }

        private void AddParagraph(Section section, HtmlNode node)
        {
            if (node == null) return;

            Paragraph para = section.AddParagraph();

            // Process the paragraph content (may contain spans, links, etc.)
            ProcessInlineElements(para, node);
        }

        private void AddImageToParaSection(string imageUrl, Section section)
        {
            try
            {
                // Decode the URL to ensure any special characters are handled properly
                string decodedUrl = HttpUtility.HtmlDecode(imageUrl);

                // Extract the file name and extension from the URL
                string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
                string tempImagePath = Path.Combine(Path.GetTempPath(), fileName);

                using (WebClient client = new WebClient())
                {
                    // Set headers to mimic a browser request (optional, but recommended)
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    // Download the image to the temporary path with the correct extension
                    client.DownloadFile(decodedUrl, tempImagePath);
                }

                // Add the image to the section
                Paragraph paragraph = section.AddParagraph();
                //paragraph.Format.LeftIndent = Unit.FromCentimeter(2);
                //paragraph.Format.RightIndent = Unit.FromCentimeter(2);
                var image = paragraph.AddImage(tempImagePath);

                // Calculate the full width of the page
                //Unit pageWidth = section.PageSetup.PageWidth;
                //Unit leftMargin = section.PageSetup.LeftMargin;
                //Unit rightMargin = section.PageSetup.RightMargin;
                //Unit fullWidth = pageWidth - leftMargin - rightMargin;

                // Set image dimensions (adjust as needed)
                image.Width = 480; // Adjust width
                image.LockAspectRatio = true; // Maintain aspect ratio
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.StackTrace}");
            }
        }

        private void AddImagetoPara(Paragraph para, HtmlNode node)
        {
            string src = node.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(src))
                return;

            try
            {
                para.Format.Alignment = ParagraphAlignment.Center;

                if (src.StartsWith("http") || src.StartsWith("https"))
                {
                    // Download external image
                    using (WebClient client = new WebClient())
                    {
                        byte[] imageData = client.DownloadData(src);
                        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
                        File.WriteAllBytes(tempPath, imageData);

                        var image = para.AddImage(tempPath);
                        image.Width = Unit.FromCentimeter(15); // Set reasonable default width
                        image.LockAspectRatio = true;

                        // Clean up temp file
                        File.Delete(tempPath);
                    }
                }
                else if (src.StartsWith("~/"))
                {
                    // Local image
                    string localPath = HttpContext.Current.Server.MapPath(src);
                    if (File.Exists(localPath))
                    {
                        var image = para.AddImage(localPath);
                        image.Width = Unit.FromCentimeter(15); // Set reasonable default width
                        image.LockAspectRatio = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error and add placeholder text
                Console.WriteLine($"Error adding image {src}: {ex.Message}");

            }
        }

        private void AddTableToSection(Section section, HtmlNode tableNode)
        {
            var table = section.AddTable();
            //table.Borders.Width = 0.75;

            // Assuming simple table with rows and cells
            var rows = tableNode.SelectNodes(".//tr");
            if (rows == null) return;

            // Count number of columns by first row
            var firstRowCells = rows[0].SelectNodes("./td|./th");
            foreach (var _ in firstRowCells)
            {
                var column = table.AddColumn(Unit.FromCentimeter(4)); // Adjust column width
                column.Format.Alignment = ParagraphAlignment.Left;
            }

            foreach (var rowNode in rows)
            {
                var row = table.AddRow();
                var cells = rowNode.SelectNodes("./td|./th");
                for (int i = 0; i < cells.Count; i++)
                {
                    var cell = row.Cells[i];
                    var cellContent = cells[i].InnerText.Trim();
                    cell.AddParagraph(WebUtility.HtmlDecode(cellContent));
                }
            }
        }




        private void ProcessInlineElements(Paragraph para, HtmlNode node, string titleText = null)
        {
            if (titleText == "")

                if (node == null) return;

            if (node.InnerText.IndexOf("Nomadix offers a Software Development Kit (SDK) for easy app integration") > 0)
            {

                string str = "";

            }

            if (titleText != null && !string.IsNullOrWhiteSpace(titleText.Trim()))
            {
                string iconFile = titleText.Trim().Equals("Note", StringComparison.OrdinalIgnoreCase)
                                   ? "~/logo/info_icon.png"
                                   : "~/logo/warning_icon.png";
                string iconPath = HttpContext.Current.Server.MapPath(iconFile);

                if (File.Exists(iconPath))
                {
                    var iconImg = para.AddImage(iconPath);
                    iconImg.Width = Unit.FromCentimeter(0.6);
                    iconImg.LockAspectRatio = true;
                    iconImg.WrapFormat.Style = WrapStyle.Through; // Changed to Inline for better text flow
                    iconImg.Top = ShapePosition.Top;
                    iconImg.Left = ShapePosition.Left;

                    // Add space between icon and title
                    para.AddText(" ");
                }
            }

            // Add title with line break if it exists
            if (titleText != null)
            {
                para.AddText(titleText);
                para.AddLineBreak();
            }


            foreach (var childNode in node.ChildNodes)
            {

                if (childNode.NodeType == HtmlNodeType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(childNode.InnerText))
                    {
                        // Add icon if titleText exists
                        // Add the main content text
                        para.AddText(WebUtility.HtmlDecode(childNode.InnerText));

                        // Format the paragraph
                        para.Format.SpaceBefore = Unit.FromPoint(6);
                        para.Format.SpaceAfter = Unit.FromPoint(6);


                    }
                }
                else if (childNode.NodeType == HtmlNodeType.Element)
                {

                    if (childNode.Name == "div" && childNode.HasClass("table-shadow-wrapper"))
                    {
                        // Handle table wrapper div
                        var tableNode = childNode.SelectSingleNode(".//table");
                        if (tableNode != null)
                        {
                            Console.WriteLine("Found table-shadow-wrapper with table, calling AddTableToDocument");
                            AddTableToDocument(para, tableNode);
                        }
                        else
                        {
                            Console.WriteLine("Found table-shadow-wrapper but no table inside");
                        }
                        continue;
                    }
                    else
                    {

                        switch (childNode.Name.ToLower())
                        {
                            case "b":
                            case "strong":
                                var boldText = para.AddFormattedText(WebUtility.HtmlDecode(childNode.InnerText));
                                boldText.Bold = true;
                                para.Format.SpaceBefore = Unit.FromPoint(6); // Add space before each list item
                                para.Format.SpaceAfter = Unit.FromPoint(6);  // Add space after each list item
                                break;
                            case "em":
                            case "i":
                                var italicText = para.AddFormattedText(WebUtility.HtmlDecode(childNode.InnerText));
                                italicText.Italic = true;
                                break;
                            case "u":
                                var underlineText = para.AddFormattedText(WebUtility.HtmlDecode(childNode.InnerText));
                                underlineText.Underline = Underline.Single;
                                break;
                            case "a":
                                var linkText = para.AddFormattedText(WebUtility.HtmlDecode(childNode.InnerText));
                                linkText.Color = new Color(0, 106, 138); // Nomadix teal color for links
                                linkText.Underline = Underline.Single;
                                break;
                            case "ul":
                                AddListNoSection(para, childNode, false);
                                break;
                            case "span":
                            case "div":
                                // Process span content recursively
                                ProcessInlineElements(para, childNode);
                                break;
                            case "br":
                                para.AddLineBreak();
                                break;
                            case "img":
                                string imageUrl = childNode.GetAttributeValue("src", string.Empty);
                                imageUrl = HttpUtility.HtmlDecode(imageUrl);

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    AddImageToParaSection(imageUrl, para.Section);
                                }
                                break;

                            default:
                                // For other elements, just add the text
                                if (!string.IsNullOrWhiteSpace(childNode.InnerText))
                                {
                                    para.AddText(WebUtility.HtmlDecode(childNode.InnerText));
                                    para.Format.SpaceBefore = Unit.FromPoint(6); // Add space before each list item
                                    para.Format.SpaceAfter = Unit.FromPoint(6);  // Add space after each list item
                                }
                                break;
                        }
                    }
                }
            }
        }
        private void AddTableToDocument(Paragraph para, HtmlNode tableNode)
        {
            var section = para.Section;
            var table = section.AddTable();
            table.Borders.Width = 0.5;

            // Determine number of columns
            var firstRow = tableNode.SelectSingleNode(".//tr");
            if (firstRow == null) return;
            var cells = firstRow.SelectNodes("./th|./td");
            if (cells == null || cells.Count == 0) return;
            int columnCount = cells.Count;

            // Calculate dynamic column widths based on content
            double[] columnWidths = CalculateColumnWidths(tableNode, columnCount);
            for (int i = 0; i < columnCount; i++)
            {
                table.AddColumn(Unit.FromCentimeter(columnWidths[i]));
            }

            // Add rows and cells
            var rows = tableNode.SelectNodes(".//tr");
            if (rows == null) return;

            foreach (var rowNode in rows)
            {
                var newRow = table.AddRow();
                var rowCells = rowNode.SelectNodes("./th|./td");
                if (rowCells == null) continue;

                for (int i = 0; i < rowCells.Count && i < table.Columns.Count; i++)
                {
                    var cell = newRow.Cells[i];
                    var paragraph = cell.AddParagraph();

                    foreach (var content in rowCells[i].ChildNodes)
                    {
                        if (content.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                        {
                            AddImageToParagraph(paragraph, content);
                        }
                        else if (content.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var pContent in content.ChildNodes)
                            {
                                if (pContent.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Direct image handling for images in paragraphs inside table cells
                                    var src = pContent.GetAttributeValue("src", "");
                                    if (!string.IsNullOrEmpty(src))
                                    {
                                        try
                                        {
                                            using (var client = new WebClient())
                                            {
                                                client.Headers.Add("User-Agent", "Mozilla/5.0");
                                                var tempPath = Path.GetTempFileName();

                                                try
                                                {
                                                    client.DownloadFile(src, tempPath);
                                                    var image = paragraph.AddImage(tempPath);

                                                    // Get width from attributes or style
                                                    var widthAttr = pContent.GetAttributeValue("width", "");
                                                    var styleAttr = pContent.GetAttributeValue("style", "");

                                                    // Extract width from style attribute if present
                                                    int styleWidth = 0;
                                                    if (!string.IsNullOrEmpty(styleAttr))
                                                    {
                                                        var widthMatch = Regex.Match(styleAttr, @"width:([0-9]+)px");
                                                        if (widthMatch.Success && widthMatch.Groups.Count > 1)
                                                        {
                                                            int.TryParse(widthMatch.Groups[1].Value, out styleWidth);
                                                        }
                                                    }

                                                    // Use explicit width attribute first, then style width, then default
                                                    if (!string.IsNullOrEmpty(widthAttr) && int.TryParse(widthAttr, out int width))
                                                    {
                                                        image.Width = Unit.FromPoint(width);
                                                    }
                                                    else if (styleWidth > 0)
                                                    {
                                                        image.Width = Unit.FromPoint(styleWidth);
                                                    }
                                                    else
                                                    {
                                                        // Default width that works well in tables
                                                        image.Width = Unit.FromCentimeter(8);
                                                    }

                                                    // Center the image in the cell
                                                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                                                    image.LockAspectRatio = true;
                                                }
                                                finally
                                                {
                                                    if (File.Exists(tempPath))
                                                    {
                                                        File.Delete(tempPath);
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            paragraph.AddText("[Image could not be loaded]");
                                            Debug.WriteLine($"Image load error: {ex.Message}");
                                        }
                                    }
                                }
                                else
                                {
                                    paragraph.AddText(WebUtility.HtmlDecode(pContent.InnerText));
                                    paragraph.Format.SpaceAfter = Unit.FromPoint(6);
                                    paragraph.Format.SpaceBefore = Unit.FromPoint(6);
                                }
                            }
                        }
                        else
                        {
                            paragraph.AddText(WebUtility.HtmlDecode(content.InnerText));
                            paragraph.Format.SpaceAfter = Unit.FromPoint(6);
                            paragraph.Format.SpaceBefore = Unit.FromPoint(6);
                        }
                    }
                }
            }
        }

        //private void AddTableToDocument(Paragraph para, HtmlNode tableNode)
        //{
        //    // Create a new table in the document
        //    var table = para.Section.AddTable();
        //    table.Borders.Width = 0.5;

        //    // Add columns based on colgroup or td counts
        //    var colgroup = tableNode.SelectSingleNode("./colgroup");
        //    if (colgroup != null)
        //    {
        //        foreach (HtmlNode col in colgroup.SelectNodes("./col"))
        //        {
        //            var width = col.GetAttributeValue("style", "").Replace("width:", "").Replace("px", "").Trim();
        //            //if (Unit.TryParse(width + "pt", out Unit colWidth))
        //            //{
        //            table.AddColumn();
        //            //}
        //            //else
        //            //{
        //            //    table.AddColumn();
        //            //}
        //        }
        //    }
        //    else
        //    {
        //        // Fallback: Add 2 equal columns
        //        table.AddColumn(Unit.FromPoint(440));
        //        table.AddColumn(Unit.FromPoint(439));
        //    }

        //    // Process rows
        //    foreach (HtmlNode row in tableNode.SelectNodes(".//tr"))
        //    {
        //        var newRow = table.AddRow();
        //        int cellIndex = 0;
        //        foreach (HtmlNode cell in row.SelectNodes("./td"))
        //        {
        //            //var cellIndex = 0;
        //            var paragraph = newRow.Cells[cellIndex].AddParagraph();

        //            // Process cell content
        //            foreach (var content in cell.ChildNodes)
        //            {
        //                if (content.Name == "img")
        //                {
        //                    AddImageToParagraph(paragraph, content);
        //                }
        //                else if (content.Name == "p")
        //                {
        //                    foreach (var pContent in content.ChildNodes)
        //                    {
        //                        if (pContent.Name == "img")
        //                        {
        //                            AddImageToParagraph(paragraph, pContent);
        //                        }
        //                        else
        //                        {
        //                            paragraph.AddText(WebUtility.HtmlDecode(pContent.InnerText));
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    paragraph.AddText(WebUtility.HtmlDecode(content.InnerText));
        //                }
        //            }
        //            cellIndex++;
        //        }
        //    }
        //}

        private void AddImageToParagraph(Paragraph para, HtmlNode imgNode)
        {
            var src = imgNode.GetAttributeValue("src", "");
            if (!string.IsNullOrEmpty(src))
            {
                try
                {
                    // Handle Azure CDN URL with SAS token
                    var uri = new Uri(src);
                    var baseUri = uri.GetLeftPart(UriPartial.Path);

                    // Download image (with timeout)
                    using (var client = new WebClient())
                    {
                        client.Headers.Add("User-Agent", "Mozilla/5.0");
                        var tempPath = Path.GetTempFileName();

                        try
                        {
                            client.DownloadFile(src, tempPath);

                            // Add to paragraph
                            var image = para.AddImage(tempPath);

                            // Get width from attributes or style
                            var widthAttr = imgNode.GetAttributeValue("width", "");
                            var styleAttr = imgNode.GetAttributeValue("style", "");

                            // Extract width from style attribute if present
                            int styleWidth = 0;
                            if (!string.IsNullOrEmpty(styleAttr))
                            {
                                var widthMatch = Regex.Match(styleAttr, @"width:([0-9]+)px");
                                if (widthMatch.Success && widthMatch.Groups.Count > 1)
                                {
                                    int.TryParse(widthMatch.Groups[1].Value, out styleWidth);
                                }
                            }

                            // Use explicit width attribute first, then style width, then default
                            if (!string.IsNullOrEmpty(widthAttr) && int.TryParse(widthAttr, out int width))
                            {
                                image.Width = Unit.FromPoint(width);
                            }
                            else if (styleWidth > 0)
                            {
                                image.Width = Unit.FromPoint(styleWidth);
                            }
                            else
                            {
                                // Default width
                                image.Width = Unit.FromCentimeter(8);
                            }

                            // Center the image in the paragraph
                            para.Format.Alignment = ParagraphAlignment.Center;
                            image.LockAspectRatio = true;
                        }
                        finally
                        {
                            if (File.Exists(tempPath))
                            {
                                File.Delete(tempPath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    para.AddText("[Image could not be loaded]");
                    Debug.WriteLine($"Image load error: {ex.Message}");
                }
            }
        }



        private void AddListNoSection(Paragraph para, HtmlNode node, bool isOrdered)
        {
            if (node == null) return;

            int itemNumber = 1;
            foreach (var listItem in node.SelectNodes("./li"))
            {
                //Paragraph para = section.AddParagraph();
                para.Format.LeftIndent = Unit.FromCentimeter(0.5);
                para.Format.SpaceBefore = Unit.FromPoint(6); // Add space before each list item
                para.Format.SpaceAfter = Unit.FromPoint(6);  // Add space after each list item
                para.Format.LineSpacing = Unit.FromPoint(10); // Add line spacing within list items

                // Add bullet or number
                if (isOrdered)
                {
                    para.AddText($"{itemNumber}. ");
                    itemNumber++;
                }
                else
                {
                    para.AddText("• ");
                }

                // Process list item content
                ProcessInlineElements(para, listItem);
            }
        }

        private void AddBlockquote(Section section, HtmlNode node)
        {
            if (node == null) return;

            // Determine if it's a note or warning blockquote
            bool isWarning = node.GetAttributeValue("class", "").Contains("warningBox");
            string styleToUse = isWarning ? "WarningBox" : "NoteBox";

            // Add title if present
            var titleNode = node.SelectSingleNode(".//div[@class='blockquote-title']") ??
                           node.SelectSingleNode(".//p[contains(., 'Warning') or contains(., 'Note')]")
                           ?? node.FirstChild;
            string titleText = isWarning ? "Warning" : "Note";
            //if (titleNode != null)
            //{
            //    Paragraph titlePara = section.AddParagraph();
            //    titlePara.Style = styleToUse;
            //    titlePara.Format.SpaceAfter = 0.7;


            //    if (!string.IsNullOrWhiteSpace(titleNode.InnerText))
            //    {
            //        titleText = WebUtility.HtmlDecode(StripHtml(titleNode.InnerText)).Trim();
            //    }

            //    var formattedTitle = titlePara.AddFormattedText(titleText);
            //    formattedTitle.Bold = true;
            //}

            // Add content
            foreach (var contentNode in node.ChildNodes)
            {
                // Skip the title node we already processed
                if (contentNode == titleNode) continue;

                if (contentNode.Name.ToLower() == "p" || contentNode.NodeType == HtmlNodeType.Text)
                {
                    Paragraph contentPara = section.AddParagraph();
                    contentPara.Style = styleToUse;
                    ProcessInlineElements(contentPara, contentNode, titleText);
                }
                else if (contentNode.Name.ToLower() == "ul")
                {
                    AddList(section, contentNode, false);
                }
            }
        }

        private void AddList(Section section, HtmlNode node, bool isOrdered)
        {
            if (node == null) return;

            int itemNumber = 1;
            foreach (var listItem in node.SelectNodes("./li"))
            {
                Paragraph para = section.AddParagraph();
                para.Format.LeftIndent = Unit.FromCentimeter(0.5);
                para.Format.SpaceBefore = Unit.FromPoint(6); // Add space before each list item
                para.Format.SpaceAfter = Unit.FromPoint(6);  // Add space after each list item
                para.Format.LineSpacing = Unit.FromPoint(10); // Add line spacing within list items

                // Add bullet or number
                if (isOrdered)
                {
                    para.AddText($"{itemNumber}. ");
                    itemNumber++;
                }
                else
                {
                    para.AddText("• ");
                }

                // Process list item content
                ProcessInlineElements(para, listItem);
            }
        }

        //private void AddTable(Section section, HtmlNode node)
        //{
        //    if (node == null) return;

        //    // Add space before table
        //    Paragraph spacer = section.AddParagraph();
        //    spacer.Format.SpaceBefore = Unit.FromCentimeter(0.5);

        //    Table table = section.AddTable();
        //    table.Borders.Width = 0.5;
        //    table.Borders.Color = Colors.Gray;

        //    // Determine number of columns
        //    var firstRow = node.SelectSingleNode(".//tr");
        //    if (firstRow == null) return;

        //    var cells = firstRow.SelectNodes(".//th|.//td");
        //    if (cells == null || cells.Count == 0) return;

        //    // Add columns to the table
        //    double columnWidth = 16.0 / cells.Count; // Distribute columns evenly (16cm total width)
        //    for (int i = 0; i < cells.Count; i++)
        //    {
        //        table.AddColumn(Unit.FromCentimeter(columnWidth));
        //    }

        //    // Process rows
        //    foreach (HtmlNode row in tableNode.SelectNodes(".//tr"))
        //    {
        //        var newRow = table.AddRow();

        //        // Process cells
        //        var cellNodes = rowNode.SelectNodes(".//th|.//td");
        //        if (cellNodes == null) continue;

        //        for (int i = 0; i < cellNodes.Count && i < table.Columns.Count; i++)
        //        {
        //            var cellNode = cellNodes[i];
        //            Cell cell = row.Cells[i];

        //            // Apply header styling
        //            if (isHeader || cellNode.Name.ToLower() == "th")
        //            {
        //                cell.Shading.Color = new Color(191, 191, 191); // Gray background for headers
        //                cell.Format.Font.Bold = true;
        //            }

        //            // Add cell content
        //            Paragraph cellPara = cell.AddParagraph();
        //            ProcessInlineElements(cellPara, cellNode);
        //        }

        //        isHeader = false; // Only first row is header
        //    }
        //}
        private void AddTable(Section section, HtmlNode node)
        {
            if (node == null) return;

            // Add page break before table header to ensure it starts on a fresh page
            Paragraph pageBreak = section.AddParagraph();
            pageBreak.Format.PageBreakBefore = true;
            pageBreak.Format.SpaceBefore = Unit.FromCentimeter(0.5);

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            // Determine number of columns
            var firstRow = node.SelectSingleNode(".//tr");
            if (firstRow == null) return;

            var cells = firstRow.SelectNodes(".//th|.//td");
            if (cells == null || cells.Count == 0) return;

            // Calculate dynamic column widths based on content
            var columnWidths = CalculateColumnWidths(node, cells.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                table.AddColumn(Unit.FromCentimeter(columnWidths[i]));
            }

            // Process rows (keep existing structure)
            var rows = node.SelectNodes(".//tr");
            if (rows == null) return;

            bool isHeader = true;
            foreach (var rowNode in rows)
            {
                // Check if this is a blank row that should be skipped
                bool isBlankRow = IsBlankRow(rowNode);
                if (isBlankRow)
                {
                    continue; // Skip this row
                }

                Row row = table.AddRow();

                // Process cells (keep existing structure)
                var cellNodes = rowNode.SelectNodes(".//th|.//td");
                if (cellNodes == null) continue;

                for (int i = 0; i < cellNodes.Count && i < table.Columns.Count; i++)
                {
                    var cellNode = cellNodes[i];
                    Cell cell = row.Cells[i];

                    // Apply header styling with standardized padding
                    if (isHeader || cellNode.Name.ToLower() == "th")
                    {
                        cell.Shading.Color = new Color(191, 191, 191);
                        cell.Format.Font.Bold = true;
                        // Header padding: 6pt top/bottom
                        cell.Format.SpaceBefore = Unit.FromPoint(6);
                        cell.Format.SpaceAfter = Unit.FromPoint(6);
                        cell.Format.LeftIndent = Unit.FromPoint(4);
                        cell.Format.RightIndent = Unit.FromPoint(4);
                    }
                    else
                    {
                        // Row padding: 4pt all sides
                        cell.Format.SpaceBefore = Unit.FromPoint(4);
                        cell.Format.SpaceAfter = Unit.FromPoint(4);
                        cell.Format.LeftIndent = Unit.FromPoint(4);
                        cell.Format.RightIndent = Unit.FromPoint(4);
                    }

                    // MODIFIED: Enhanced cell content processing
                    ProcessTableCellContent(cell, cellNode);
                }

                isHeader = false;
            }
        }

        // New helper method to handle table cell content
        private void ProcessTableCellContent(Cell cell, HtmlNode cellNode)
        {
            // Process each child node in the cell
            foreach (var content in cellNode.ChildNodes)
            {
                if (content.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle images with proper downloading
                    string imageUrl = content.GetAttributeValue("src", "");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        try
                        {
                            // Decode the URL to ensure any special characters are handled properly
                            string decodedUrl = HttpUtility.HtmlDecode(imageUrl);

                            // Extract the file name and extension from the URL
                            string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
                            string tempImagePath = Path.Combine(Path.GetTempPath(), fileName);

                            using (WebClient client = new WebClient())
                            {
                                // Set headers to mimic a browser request
                                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                                // Download the image using the full URL with query parameters
                                client.DownloadFile(decodedUrl, tempImagePath);
                            }

                            // Add image to cell
                            Paragraph imagePara = cell.AddParagraph();
                            MigraDoc.DocumentObjectModel.Shapes.Image image = imagePara.AddImage(tempImagePath);

                            // Set image dimensions if specified
                            string width = content.GetAttributeValue("width", "");
                            string height = content.GetAttributeValue("height", "");

                            // Width handling
                            if (!string.IsNullOrEmpty(width))
                            {
                                if (width == "auto")
                                {
                                    // Get column width by finding the cell's position in the row
                                    int colIndex = cell.Column.Index;
                                    if (colIndex >= 0 && colIndex < cell.Table.Columns.Count)
                                    {
                                        image.Width = Unit.FromCentimeter(cell.Table.Columns[colIndex].Width.Centimeter - 0.2);
                                    }
                                }
                                else if (int.TryParse(width.Replace("px", ""), out int pxWidth))
                                {
                                    image.Width = Unit.FromCentimeter(pxWidth * 0.026458); // px to cm
                                }
                            }

                            // Height handling
                            if (!string.IsNullOrEmpty(height))
                            {
                                if (height == "auto")
                                {
                                    image.LockAspectRatio = true;
                                }
                                else if (int.TryParse(height.Replace("px", ""), out int pxHeight))
                                {
                                    image.Height = Unit.FromCentimeter(pxHeight * 0.026458); // px to cm
                                }
                            }

                            // Center the image
                            imagePara.Format.Alignment = ParagraphAlignment.Center;

                            // Clean up temp file
                            try
                            {
                                if (File.Exists(tempImagePath))
                                {
                                    File.Delete(tempImagePath);
                                }
                            }
                            catch { /* Ignore cleanup errors */ }
                        }
                        catch (Exception ex)
                        {
                            // Add error text if image fails to load
                            var errorPara = cell.AddParagraph();
                            errorPara.AddText("[Image could not be loaded]");
                        }
                    }
                }
                else if (content.Name.Equals("h1", StringComparison.OrdinalIgnoreCase) ||
                        content.Name.Equals("h2", StringComparison.OrdinalIgnoreCase) ||
                        content.Name.Equals("h3", StringComparison.OrdinalIgnoreCase) ||
                        content.Name.Equals("h4", StringComparison.OrdinalIgnoreCase) ||
                        content.Name.Equals("h5", StringComparison.OrdinalIgnoreCase) ||
                        content.Name.Equals("h6", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle headings in table cells
                    var headingPara = cell.AddParagraph();

                    // Process any text content directly in the heading
                    var textNodes = content.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text);
                    foreach (var textNode in textNodes)
                    {
                        if (!string.IsNullOrWhiteSpace(textNode.InnerText))
                        {
                            headingPara.AddText(WebUtility.HtmlDecode(textNode.InnerText.Trim()));
                        }
                    }

                    // Process any span elements in the heading
                    var spanNodes = content.SelectNodes(".//span");
                    if (spanNodes != null)
                    {
                        foreach (var spanNode in spanNodes)
                        {
                            if (!string.IsNullOrWhiteSpace(spanNode.InnerText))
                            {
                                // Get color and font size from style attribute if present
                                string style = spanNode.GetAttributeValue("style", "");
                                string colorValue = ExtractStyleValue(style, "color");
                                string fontSizeValue = ExtractStyleValue(style, "font-size");

                                var textFormat = new MigraDoc.DocumentObjectModel.Font();

                                // Apply color if specified
                                if (!string.IsNullOrEmpty(colorValue))
                                {
                                    Color color = ParseColor(colorValue);
                                    textFormat.Color = color;
                                }

                                // Apply font size if specified
                                if (!string.IsNullOrEmpty(fontSizeValue))
                                {
                                    int fontSize = ParseFontSize(fontSizeValue);
                                    if (fontSize > 0)
                                    {
                                        textFormat.Size = fontSize;
                                    }
                                }

                                // Add the text with formatting
                                FormattedText formattedText = headingPara.AddFormattedText(WebUtility.HtmlDecode(spanNode.InnerText.Trim()));
                                if (!string.IsNullOrEmpty(colorValue))
                                    formattedText.Font.Color = textFormat.Color;
                                if (!string.IsNullOrEmpty(fontSizeValue) && textFormat.Size > 0)
                                    formattedText.Font.Size = textFormat.Size;
                            }
                        }
                    }

                    // Apply heading styles based on level
                    switch (content.Name.ToLower())
                    {
                        case "h1":
                            headingPara.Format.Font.Size = 18;
                            headingPara.Format.Font.Bold = true;
                            headingPara.Format.SpaceAfter = Unit.FromPoint(12);
                            break;
                        case "h2":
                            headingPara.Format.Font.Size = 16;
                            headingPara.Format.Font.Bold = true;
                            headingPara.Format.SpaceBefore = Unit.FromPoint(8); // IS-003: 8pt before
                            headingPara.Format.SpaceAfter = Unit.FromPoint(6);  // IS-003: 6pt after
                            break;
                        case "h3":
                            headingPara.Format.Font.Size = 14;
                            headingPara.Format.Font.Bold = true;
                            headingPara.Format.SpaceBefore = Unit.FromPoint(12); // 12pt padding before subheadings
                            headingPara.Format.SpaceAfter = Unit.FromPoint(6);   // 6pt after for clear separation
                            break;
                        default:
                            headingPara.Format.Font.Size = 12;
                            headingPara.Format.Font.Bold = true;
                            headingPara.Format.SpaceAfter = Unit.FromPoint(6);
                            break;
                    }

                    // Process any nested lists in the heading
                    var ulNodes = content.SelectNodes(".//ul");
                    if (ulNodes != null)
                    {
                        foreach (var ulNode in ulNodes)
                        {
                            // Process each list item
                            var liNodes = ulNode.SelectNodes("./li"); // Direct children only
                            if (liNodes != null)
                            {
                                foreach (var liNode in liNodes)
                                {
                                    var listItemPara = cell.AddParagraph();
                                    listItemPara.Format.LeftIndent = Unit.FromCentimeter(0.5);

                                    // Process text content in list item
                                    var liTextNodes = liNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text);
                                    foreach (var liTextNode in liTextNodes)
                                    {
                                        if (!string.IsNullOrWhiteSpace(liTextNode.InnerText))
                                        {
                                            listItemPara.AddText("• " + WebUtility.HtmlDecode(liTextNode.InnerText.Trim()));
                                        }
                                    }

                                    // Process span elements in list item
                                    var liSpanNodes = liNode.SelectNodes("./span");
                                    if (liSpanNodes != null)
                                    {
                                        foreach (var liSpanNode in liSpanNodes)
                                        {
                                            if (!string.IsNullOrWhiteSpace(liSpanNode.InnerText))
                                            {
                                                string style = liSpanNode.GetAttributeValue("style", "");
                                                string colorValue = ExtractStyleValue(style, "color");
                                                string fontSizeValue = ExtractStyleValue(style, "font-size");

                                                FormattedText formattedText = listItemPara.AddFormattedText("• " + WebUtility.HtmlDecode(liSpanNode.InnerText.Trim()));

                                                if (!string.IsNullOrEmpty(colorValue))
                                                    formattedText.Font.Color = ParseColor(colorValue);

                                                if (!string.IsNullOrEmpty(fontSizeValue))
                                                {
                                                    int fontSize = ParseFontSize(fontSizeValue);
                                                    if (fontSize > 0)
                                                        formattedText.Font.Size = fontSize;
                                                }
                                            }
                                        }
                                    }

                                    listItemPara.Format.SpaceBefore = Unit.FromPoint(6);
                                    listItemPara.Format.SpaceAfter = Unit.FromPoint(6);
                                }
                            }
                        }
                    }
                }
                else if (content.Name.Equals("ul", StringComparison.OrdinalIgnoreCase))
                {
                    // Process list items directly in table cell
                    var liNodes = content.SelectNodes("./li"); // Direct children only
                    if (liNodes != null)
                    {
                        foreach (var liNode in liNodes)
                        {
                            var listItemPara = cell.AddParagraph();
                            listItemPara.Format.LeftIndent = Unit.FromCentimeter(0.5);

                            // Check if the list item has paragraph children
                            var pNodes = liNode.SelectNodes("./p");
                            if (pNodes != null && pNodes.Count > 0)
                            {
                                // Process paragraphs within list item
                                foreach (var pNode in pNodes)
                                {
                                    // Add bullet point to the first paragraph only
                                    if (pNode == pNodes[0])
                                    {
                                        listItemPara.AddText("• " + WebUtility.HtmlDecode(pNode.InnerText.Trim()));
                                    }
                                    else
                                    {
                                        var additionalPara = cell.AddParagraph();
                                        additionalPara.Format.LeftIndent = Unit.FromCentimeter(0.7); // Indent slightly more
                                        additionalPara.AddText(WebUtility.HtmlDecode(pNode.InnerText.Trim()));
                                        additionalPara.Format.SpaceAfter = Unit.FromPoint(3);
                                    }
                                }
                            }
                            else
                            {
                                // Process text content in list item
                                var liTextNodes = liNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text);
                                foreach (var liTextNode in liTextNodes)
                                {
                                    if (!string.IsNullOrWhiteSpace(liTextNode.InnerText))
                                    {
                                        listItemPara.AddText("• " + WebUtility.HtmlDecode(liTextNode.InnerText.Trim()));
                                    }
                                }

                                // Process span elements in list item
                                var liSpanNodes = liNode.SelectNodes(".//span");
                                if (liSpanNodes != null)
                                {
                                    foreach (var liSpanNode in liSpanNodes)
                                    {
                                        if (!string.IsNullOrWhiteSpace(liSpanNode.InnerText))
                                        {
                                            string style = liSpanNode.GetAttributeValue("style", "");
                                            string colorValue = ExtractStyleValue(style, "color");
                                            string fontSizeValue = ExtractStyleValue(style, "font-size");

                                            // Only add bullet if this is the first span and no text has been added yet
                                            string prefix = listItemPara.Elements.Count == 0 ? "• " : "";
                                            FormattedText formattedText = listItemPara.AddFormattedText(prefix + WebUtility.HtmlDecode(liSpanNode.InnerText.Trim()));

                                            if (!string.IsNullOrEmpty(colorValue))
                                                formattedText.Font.Color = ParseColor(colorValue);

                                            if (!string.IsNullOrEmpty(fontSizeValue))
                                            {
                                                int fontSize = ParseFontSize(fontSizeValue);
                                                if (fontSize > 0)
                                                    formattedText.Font.Size = fontSize;
                                            }
                                        }
                                    }
                                }
                            }

                            listItemPara.Format.SpaceBefore = Unit.FromPoint(6);
                            listItemPara.Format.SpaceAfter = Unit.FromPoint(6);
                        }
                    }
                }
                else if (content.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle paragraphs in table cells
                    var paragraph = cell.AddParagraph();

                    // Check if paragraph contains an image
                    var imgNode = content.SelectSingleNode(".//img");
                    if (imgNode != null)
                    {
                        // Handle image within paragraph
                        string imageUrl = imgNode.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            try
                            {
                                try
                                {
                                    // Decode the URL to ensure any special characters are handled properly
                                    string decodedUrl = HttpUtility.HtmlDecode(imageUrl);

                                    // Extract the file name and extension from the URL
                                    string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
                                    string tempImagePath = Path.Combine(Path.GetTempPath(), fileName);

                                    using (WebClient client = new WebClient())
                                    {
                                        // Set headers to mimic a browser request (optional, but recommended)
                                        client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                                        // Download the image to the temporary path with the correct extension
                                        client.DownloadFile(decodedUrl, tempImagePath);
                                    }

                                    // Add the image to the section
                                    //Paragraph paragraph = section.AddParagraph();
                                    //paragraph.Format.LeftIndent = Unit.FromCentimeter(2);
                                    //paragraph.Format.RightIndent = Unit.FromCentimeter(2);
                                    var image = paragraph.AddImage(tempImagePath);

                                    // Calculate the full width of the page
                                    //Unit pageWidth = section.PageSetup.PageWidth;
                                    //Unit leftMargin = section.PageSetup.LeftMargin;
                                    //Unit rightMargin = section.PageSetup.RightMargin;
                                    //Unit fullWidth = pageWidth - leftMargin - rightMargin;

                                    // Set image dimensions based on cell width
                                    // Get column width by finding the cell's position in the row
                                    int colIndex = cell.Column.Index;
                                    if (colIndex >= 0 && colIndex < cell.Table.Columns.Count)
                                    {
                                        // Set image width to slightly less than cell width to prevent overflow
                                        image.Width = Unit.FromCentimeter(cell.Table.Columns[colIndex].Width.Centimeter - 0.5);
                                    }
                                    else
                                    {
                                        // Fallback to a reasonable default width if column index is invalid
                                        image.Width = Unit.FromCentimeter(5);
                                    }
                                    image.LockAspectRatio = true; // Maintain aspect ratio


                                    //paragraph.AddText("Added Image");
                                }
                                catch (Exception ex)
                                {
                                    paragraph.AddText("Added Image");
                                }
                            }
                            catch (Exception ex)
                            {
                                // Add error text if image fails to load
                                //paragraph.AddText("[Image could not be loaded]");
                                paragraph.AddText(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        // Regular paragraph without image
                        paragraph.AddText(WebUtility.HtmlDecode(content.InnerText.Trim()));

                    }
                    paragraph.Format.SpaceBefore = Unit.FromPoint(6);
                    paragraph.Format.SpaceAfter = Unit.FromPoint(6);
                }
                else if (content.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(content.InnerText))
                {
                    // Handle plain text nodes
                    var paragraph = cell.AddParagraph();
                    paragraph.AddText(WebUtility.HtmlDecode(content.InnerText.Trim()));
                    paragraph.Format.SpaceAfter = Unit.FromPoint(0.3);
                    paragraph.Format.SpaceBefore = Unit.FromPoint(0.3);
                }
            }
        }

        private void AddHorizontalLine(Section section)
        {
            Paragraph para = section.AddParagraph();
            para.Format.Borders.Bottom.Width = 0.5;
            para.Format.Borders.Bottom.Color = new Color(28, 74, 113); // Nomadix blue color
            para.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            para.Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }

        private void AddImage(Section section, HtmlNode node)
        {
            string src = node.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(src))
                return;

            try
            {
                Paragraph para = section.AddParagraph();
                para.Format.Alignment = ParagraphAlignment.Center;

                if (src.StartsWith("http") || src.StartsWith("https"))
                {
                    // Download external image
                    using (WebClient client = new WebClient())
                    {
                        byte[] imageData = client.DownloadData(src);
                        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(src));
                        File.WriteAllBytes(tempPath, imageData);

                        var image = para.AddImage(tempPath);
                        image.Width = Unit.FromCentimeter(15); // Set reasonable default width
                        image.LockAspectRatio = true;

                        // Clean up temp file
                        File.Delete(tempPath);
                    }
                }
                else if (src.StartsWith("~/"))
                {
                    // Local image
                    string localPath = HttpContext.Current.Server.MapPath(src);
                    if (File.Exists(localPath))
                    {
                        var image = para.AddImage(localPath);
                        image.Width = Unit.FromCentimeter(15); // Set reasonable default width
                        image.LockAspectRatio = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error and add placeholder text
                Console.WriteLine($"Error adding image {src}: {ex.Message}");

            }
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // Simple HTML tag removal
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }

        // Helper method to extract style values from style attribute
        private string ExtractStyleValue(string style, string property)
        {
            if (string.IsNullOrEmpty(style))
                return string.Empty;

            // Create regex pattern to match the property and its value
            string pattern = property + "\\s*:\\s*([^;]+)";
            var match = System.Text.RegularExpressions.Regex.Match(style, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success && match.Groups.Count > 1)
                return match.Groups[1].Value.Trim();

            return string.Empty;
        }

        // Helper method to parse color values from CSS color formats
        private Color ParseColor(string colorValue)
        {
            if (string.IsNullOrEmpty(colorValue))
                return Colors.Black;

            // Handle rgb() format
            if (colorValue.StartsWith("rgb("))
            {
                // Extract the RGB values
                string rgbValues = colorValue.Substring(4, colorValue.Length - 5);
                string[] values = rgbValues.Split(',');

                if (values.Length >= 3)
                {
                    int r = int.Parse(values[0].Trim());
                    int g = int.Parse(values[1].Trim());
                    int b = int.Parse(values[2].Trim());

                    // Cast to byte to match Color constructor parameter type
                    return new Color((byte)Math.Min(r, 255), (byte)Math.Min(g, 255), (byte)Math.Min(b, 255));
                }
            }
            // Handle hex format
            else if (colorValue.StartsWith("#"))
            {
                string hex = colorValue.Substring(1);
                if (hex.Length == 3) // Short hex format #RGB
                {
                    hex = new string(new char[] { hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] });
                }

                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);

                    // Cast to byte to match Color constructor parameter type
                    return new Color((byte)Math.Min(r, 255), (byte)Math.Min(g, 255), (byte)Math.Min(b, 255));
                }
            }
            // Handle named colors
            else
            {
                switch (colorValue.ToLower())
                {
                    case "red": return Colors.Red;
                    case "blue": return Colors.Blue;
                    case "green": return Colors.Green;
                    case "black": return Colors.Black;
                    case "white": return Colors.White;
                    case "gray": return Colors.Gray;
                    case "yellow": return Colors.Yellow;
                    case "orange": return Colors.Orange;
                    case "purple": return Colors.Purple;
                    case "brown": return Colors.Brown;
                        // Add more named colors as needed
                }
            }

            // Default to black if color parsing fails
            return Colors.Black;
        }

        // Helper method to parse font size values
        private int ParseFontSize(string fontSizeValue)
        {
            if (string.IsNullOrEmpty(fontSizeValue))
                return 0;

            // Handle pixel values
            if (fontSizeValue.EndsWith("px"))
            {
                if (int.TryParse(fontSizeValue.Replace("px", "").Trim(), out int pxSize))
                {
                    // Convert pixels to points (approximate conversion)
                    return pxSize * 3 / 4;
                }
            }
            // Handle point values
            else if (fontSizeValue.EndsWith("pt"))
            {
                if (int.TryParse(fontSizeValue.Replace("pt", "").Trim(), out int ptSize))
                {
                    return ptSize;
                }
            }
            // Handle em values (relative to parent)
            else if (fontSizeValue.EndsWith("em"))
            {
                if (double.TryParse(fontSizeValue.Replace("em", "").Trim(), out double emSize))
                {
                    // Assuming base font size is 12pt
                    return (int)(12 * emSize);
                }
            }
            // Handle direct numeric values
            else if (int.TryParse(fontSizeValue.Trim(), out int directSize))
            {
                return directSize;
            }
            return 0;
        }

        // Helper method to check if a table row is blank and should be skipped
        private bool IsBlankRow(HtmlNode rowNode)
        {
            if (rowNode == null) return true;

            // Get all cells in the row
            var cellNodes = rowNode.SelectNodes("./td|./th");
            if (cellNodes == null || cellNodes.Count == 0) return true;

            // Check each cell to see if it's empty or contains only empty elements
            foreach (var cellNode in cellNodes)
            {
                // Check for horizontal rule (hr) which indicates a divider row
                var hrNodes = cellNode.SelectNodes(".//hr");
                if (hrNodes != null && hrNodes.Count > 0)
                {
                    // Check if there's only an hr and possibly an empty paragraph
                    var paragraphs = cellNode.SelectNodes(".//p");
                    if (paragraphs != null)
                    {
                        bool allEmpty = true;
                        foreach (var p in paragraphs)
                        {
                            // Check if paragraph has any content
                            if (!string.IsNullOrWhiteSpace(p.InnerText) || p.SelectNodes(".//img") != null)
                            {
                                allEmpty = false;
                                break;
                            }
                        }

                        if (allEmpty)
                        {
                            // This is a row with just an hr and empty paragraphs - skip it
                            return true;
                        }
                    }
                    else
                    {
                        // Just an hr with no paragraphs
                        return true;
                    }
                }

                // Check if cell has any non-whitespace content
                if (!string.IsNullOrWhiteSpace(cellNode.InnerText))
                {
                    return false; // Cell has content, row is not blank
                }

                // Check for images or other elements that might not have text
                if (cellNode.SelectNodes(".//img") != null ||
                    cellNode.SelectNodes(".//table") != null ||
                    cellNode.SelectNodes(".//ul") != null ||
                    cellNode.SelectNodes(".//ol") != null)
                {
                    return false; // Cell has non-text content, row is not blank
                }
            }

            // If we get here, all cells were empty
            return true;
        }

        // Helper method to calculate dynamic column widths based on content
        private double[] CalculateColumnWidths(HtmlNode tableNode, int columnCount)
        {
            var columnWidths = new double[columnCount];
            var maxContentLengths = new int[columnCount];
            
            // Calculate available width (page width - left/right margins)
            double availableWidth = 16.0; // Standard A4 width minus margins
            
            // Analyze all rows to find maximum content length per column
            var rows = tableNode.SelectNodes(".//tr");
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//th|.//td");
                    if (cells != null)
                    {
                        for (int i = 0; i < cells.Count && i < columnCount; i++)
                        {
                            string cellText = cells[i].InnerText?.Trim() ?? "";
                            // Remove HTML entities and get actual text length
                            cellText = System.Net.WebUtility.HtmlDecode(cellText);
                            maxContentLengths[i] = Math.Max(maxContentLengths[i], cellText.Length);
                        }
                    }
                }
            }
            
            // Calculate total content weight
            int totalContentLength = maxContentLengths.Sum();
            
            if (totalContentLength == 0)
            {
                // If no content, distribute equally
                double equalWidth = availableWidth / columnCount;
                for (int i = 0; i < columnCount; i++)
                {
                    columnWidths[i] = equalWidth;
                }
            }
            else
            {
                // Distribute width based on content length with minimum and maximum constraints
                double minColumnWidth = 2.0; // Minimum 2cm per column
                double maxColumnWidth = availableWidth * 0.6; // Maximum 60% of available width
                
                for (int i = 0; i < columnCount; i++)
                {
                    // Calculate proportional width based on content
                    double proportionalWidth = (double)maxContentLengths[i] / totalContentLength * availableWidth;
                    
                    // Apply constraints
                    columnWidths[i] = Math.Max(minColumnWidth, Math.Min(maxColumnWidth, proportionalWidth));
                }
                
                // Ensure total width doesn't exceed available width
                double totalCalculatedWidth = columnWidths.Sum();
                if (totalCalculatedWidth > availableWidth)
                {
                    // Scale down proportionally
                    double scaleFactor = availableWidth / totalCalculatedWidth;
                    for (int i = 0; i < columnCount; i++)
                    {
                        columnWidths[i] *= scaleFactor;
                    }
                }
                else if (totalCalculatedWidth < availableWidth)
                {
                    // Distribute remaining width equally
                    double remainingWidth = availableWidth - totalCalculatedWidth;
                    double additionalWidth = remainingWidth / columnCount;
                    for (int i = 0; i < columnCount; i++)
                    {
                        columnWidths[i] += additionalWidth;
                    }
                }
            }
            
            return columnWidths;
        }

    }

    //public class HtmlRequestModel
    //{
    //    public string HtmlContent { get; set; }
    //}
}