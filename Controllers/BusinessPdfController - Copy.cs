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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PickupAPi.Controllers
{
    //[EnableCors(origins: "https://nomadix-kms.document360.io", headers: "*", methods: "*")]
    [RoutePrefix("api/BusinessPdf")]
    public class BusinessPdfController : ApiController
    {
        private readonly List<(string Title, int PageNumber)> blockquoteIndex = new List<(string, int)>();
        bool isFirst = true;
        [HttpPost]
        [Route("GeneratePdf")]
        public async Task<HttpResponseMessage> GeneratePdf([FromBody] HtmlRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.htmlContent))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid HTML Content");

            try
            {
                byte[] pdfBytes = GenerateBusinessPdf(request.htmlContent);

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

        public byte[] GenerateBusinessPdf(string htmlContent)
        {
            Document doc = new Document();
            DefineStyles(doc);

            // Add cover page
            //Section coverSection = doc.AddSection();
            AddCoverPage(doc);

            // Add content section with header and footer
            Section contentSection = doc.AddSection();
            //AddHeader(contentSection);
            AddFooter(contentSection);

            // Define page setup for content
            PageSetup pageSetup = contentSection.PageSetup;
            pageSetup.PageWidth = Unit.FromCentimeter(21);    // A4 width
            pageSetup.PageHeight = Unit.FromCentimeter(29.7); // A4 height
            pageSetup.TopMargin = Unit.FromCentimeter(1);   // Reserve space for header
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

        private void AddCoverPage1(Section section)
        {
            // Set cover page background
            section.PageSetup.PageWidth = Unit.FromCentimeter(21);    // A4 width
            section.PageSetup.PageHeight = Unit.FromCentimeter(29.7); // A4 height
            section.PageSetup.TopMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2.5);

            // Add logo
            Paragraph logoParagraph = section.AddParagraph();
            logoParagraph.Format.SpaceBefore = Unit.FromCentimeter(3);
            logoParagraph.Format.Alignment = ParagraphAlignment.Center;
            Image logo = logoParagraph.AddImage(HttpContext.Current.Server.MapPath("~/logo/nomadix_logo.png"));
            logo.Width = Unit.FromCentimeter(10);

            // Add title
            Paragraph title = section.AddParagraph("Nomadix Cloud Platform");
            title.Format.Font.Size = 28;
            title.Format.Font.Bold = true;
            title.Format.Font.Color = new Color(28, 74, 113); // Nomadix blue color
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceBefore = Unit.FromCentimeter(5);
            title.Format.SpaceAfter = Unit.FromCentimeter(1);

            // Add subtitle
            Paragraph subtitle = section.AddParagraph("Administration Guide");
            subtitle.Format.Font.Size = 20;
            subtitle.Format.Font.Color = new Color(28, 74, 113); // Nomadix blue color
            subtitle.Format.Alignment = ParagraphAlignment.Center;
            subtitle.Format.SpaceAfter = Unit.FromCentimeter(10);

            // Add date
            Paragraph date = section.AddParagraph("November 2023");
            date.Format.Font.Size = 12;
            date.Format.Alignment = ParagraphAlignment.Center;
            date.Format.SpaceBefore = Unit.FromCentimeter(5);
        }

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
            leftText.Format.Font.Name = "Arial";
            leftText.Format.Font.Size = 11;
            leftText.Format.Font.Color = Colors.White;
            leftText.Format.Alignment = ParagraphAlignment.Left;
            leftText.Format.LeftIndent = Unit.FromCentimeter(0.5); // Adjust left indent

            // Set vertical alignment for the left cell
            footerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;



            // Adjust the height of the row to fit content
            footerRow.Height = Unit.FromCentimeter(1.4);



        }

        private void AddHeader(Section section)
        {
            HeaderFooter header = section.Headers.Primary;

            // Create a table for the header
            Table headerTable = header.AddTable();
            headerTable.Borders.Width = 0;
            headerTable.AddColumn(Unit.FromCentimeter(6)); // Logo column
            headerTable.AddColumn(Unit.FromCentimeter(6)); // Title column

            Row headerRow = headerTable.AddRow();

            // Add logo to the left
            Paragraph logoParagraph = headerRow.Cells[0].AddParagraph();
            Image logo = logoParagraph.AddImage(HttpContext.Current.Server.MapPath("~/logo/nomadix_logo.png"));
            logo.Width = Unit.FromCentimeter(5);
            logoParagraph.Format.Alignment = ParagraphAlignment.Left;

            // Add title to the right
            Paragraph titleParagraph = headerRow.Cells[1].AddParagraph("Nomadix Cloud Platform");
            titleParagraph.Format.Font.Size = 12;
            titleParagraph.Format.Font.Bold = true;
            titleParagraph.Format.Font.Color = new Color(28, 74, 113); // Nomadix blue color
            titleParagraph.Format.Alignment = ParagraphAlignment.Right;

            // Add a horizontal line with proper spacing
            //Paragraph line = header.AddParagraph();
            //line.Format.Borders.Bottom.Width = 0.75;
            //line.Format.Borders.Bottom.Color = new Color(28, 74, 113); // Nomadix blue color
            //line.Format.SpaceAfter = Unit.FromCentimeter(0.5); // Increased space after the line
            //line.Format.SpaceBefore = Unit.FromCentimeter(0.2); // Added space before the line
        }

        private void AddFooter(Section section)
        {
            HeaderFooter footer = section.Footers.Primary;

            // Create a table for the footer
            Table footerTable = footer.AddTable();
            footerTable.Borders.Width = 0;
            footerTable.AddColumn(Unit.FromCentimeter(8)); // Copyright column
            footerTable.AddColumn(Unit.FromCentimeter(8)); // Page number column

            Row footerRow = footerTable.AddRow();

            // Add copyright to the left
            Paragraph copyrightParagraph = footerRow.Cells[0].AddParagraph("© NOMADIX " + DateTime.Now.Year);
            copyrightParagraph.Format.Font.Size = 10;
            copyrightParagraph.Format.Alignment = ParagraphAlignment.Left;

            // Add page number to the right
            Paragraph pageNumberParagraph = footerRow.Cells[1].AddParagraph();
            pageNumberParagraph.AddText("Page ");
            pageNumberParagraph.AddPageField();
            pageNumberParagraph.AddText(" of ");
            pageNumberParagraph.AddNumPagesField();
            pageNumberParagraph.Format.Font.Size = 10;
            pageNumberParagraph.Format.Alignment = ParagraphAlignment.Right;

            // Add a horizontal line
            //Paragraph line = footer.AddParagraph();
            //line.Format.Borders.Top.Width = 0.75;
            //line.Format.Borders.Top.Color = new Color(28, 74, 113); // Nomadix blue color
        }

        private void DefineStyles(Document doc)
        {
            // Normal text style
            Style normal = doc.Styles["Normal"];
            normal.Font.Name = "Arial";
            normal.Font.Size = 10;

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
            heading2.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.4);
            heading2.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

            Style heading3 = doc.Styles["Heading3"];
            heading3.Font.Name = "Arial";
            heading3.Font.Size = 12;
            heading3.Font.Bold = true;
            heading3.Font.Color = new Color(24, 24, 27); // Nomadix blue color
            heading3.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.3);
            heading3.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.1);

            // Create custom styles for notes and warnings
            Style noteStyle = doc.Styles.AddStyle("NoteBox", "Normal");
            noteStyle.ParagraphFormat.Borders.Width = 0.5;
            noteStyle.ParagraphFormat.Borders.Color = new Color(0, 106, 138); // Nomadix teal color
            noteStyle.ParagraphFormat.Borders.Distance = 3;
            noteStyle.ParagraphFormat.Shading.Color = new Color(28, 74, 113); // Nomadix blue background
            noteStyle.ParagraphFormat.LeftIndent = 10;
            noteStyle.ParagraphFormat.RightIndent = 10;
            noteStyle.Font.Color = Colors.White;
            noteStyle.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.6);
            noteStyle.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.4);

            Style warningStyle = doc.Styles.AddStyle("WarningBox", "Normal");
            warningStyle.ParagraphFormat.Borders.Width = 0.5;
            warningStyle.ParagraphFormat.Borders.Color = new Color(127, 100, 22); // Warning border color
            warningStyle.ParagraphFormat.Borders.Distance = 3;
            warningStyle.ParagraphFormat.Shading.Color = new Color(253, 242, 206); // Warning background color
            warningStyle.ParagraphFormat.LeftIndent = 10;
            warningStyle.ParagraphFormat.RightIndent = 10;
            warningStyle.Font.Color = new Color(127, 100, 22); // Warning text color
            warningStyle.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.6);
            warningStyle.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.4);

            // Table styles
            Style tableStyle = doc.Styles.AddStyle("Table", "Normal");
            tableStyle.Font.Name = "Arial";
            tableStyle.Font.Size = 9;
            tableStyle.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(1.0); // Increased space before table
            tableStyle.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.8);  // Increased space after table
            tableStyle.ParagraphFormat.LeftIndent = Unit.FromPoint(3); // Add cell padding
            tableStyle.ParagraphFormat.RightIndent = Unit.FromPoint(3);
            tableStyle.ParagraphFormat.LineSpacing = Unit.FromPoint(14); // Add line spacing within table

            Style tableHeader = doc.Styles.AddStyle("TableHeader", "Table");
            tableHeader.Font.Bold = true;
            tableHeader.Font.Name = "Arial"; // Ensure Arial font in headers
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

                if (childNode.InnerText.IndexOf("Nomadix offers a Software Development Kit (SDK) for easy app integration") > 0)
                {
                    string str = "";
                }


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
                            break;
                        case "h3":
                            AddHeading(section, childNode.InnerText, "Heading3");
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

                            if (childNode.InnerText.Trim().IndexOf("Notes:") >= 0)
                            {
                                string str = "";                            
                                type = HttpContext.Current.Server.MapPath("~/logo/info_icon.png");
                                //AddSpace(section);
                                AddStyledSection(section, "Note", childNode.InnerText.Trim(), Colors.DarkBlue, Colors.White, type);
                            }
                            else if (childNode.InnerText.Trim().IndexOf("Warning:") >= 0)
                            {

                                type = HttpContext.Current.Server.MapPath("~/logo/warning_icon.png");
                                AddStyledSection(section, "Warning", childNode.InnerText.Trim(), Colors.DarkOrange, Colors.White, type);
                            }
                            // Add the blockquote title to the index list
                            blockquoteIndex.Add((node.InnerText.Trim(), 0)); // Page number will be updated later     
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
            // Create a table to structure the section
            Table table = section.AddTable();
            table.Borders.Width = 0; // No border for the table

            // Add columns: one for the icon, one for the text content
            table.AddColumn(Unit.FromCentimeter(1.5)); // Icon column
            table.AddColumn(Unit.FromCentimeter(14)); // Text content column
            //table.Rows.LeftIndent = Unit.FromCentimeter(2); // Indent the table
            // Add a row to the table
            Row row = table.AddRow();

            // Set the background color for the row
            row.Shading.Color = backgroundColor;

            // Add the icon cell
            Cell iconCell = row.Cells[0];
            iconCell.Format.SpaceBefore = Unit.FromCentimeter(0.3); // Adjust spacing as needed
            iconCell.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
            {
                try
                {
                    // Add the image to the icon cell
                    var iconImage = iconCell.AddImage(iconPath);
                    iconImage.LockAspectRatio = true; // Maintain aspect ratio
                    iconImage.Width = Unit.FromCentimeter(1); // Adjust the width
                    iconImage.Height = Unit.FromCentimeter(1); // Adjust the height
                    iconImage.Left = ShapePosition.Center; // Center the image horizontally
                    iconCell.VerticalAlignment = VerticalAlignment.Center; // Center the image vertically                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding icon image: {ex.Message}");
                }
            }

            // Add the text content cell (title and content)
            Cell contentCell = row.Cells[1];


            // Content
            Paragraph contentParagraph = contentCell.AddParagraph();
            //string updatedContent = Regex.Replace(content, @"ℹ️", "").Replace("\u26A0\uFE0F", "");
            string updatedContent = content.Replace("\u2139\uFE0F", "") // Replace ℹ️
                               .Replace("\u26A0\uFE0F", ""); // Replace ⚠️

            contentParagraph.AddFormattedText(title == "Note" ? "Warning:" : "Notes:", TextFormat.Bold); // Title in Bold
            contentParagraph.AddLineBreak();
            if (title == "Note")
            {
                contentParagraph.AddText(updatedContent.Replace("Notes:", ""));
            }
            else
            {
                contentParagraph.AddText(updatedContent.Replace("Warning:", ""));
            }

            contentParagraph.Format.Font.Color = textColor;
            contentParagraph.Format.Font.Size = 10; // Adjust font size for the content
            contentParagraph.Format.Font.Name = "Arial";
            contentParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.3); // Adjust spacing as needed
            contentParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            contentCell.VerticalAlignment = VerticalAlignment.Center;

            // Add spacing after the section
            Paragraph spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(0.2); // Adjust spacing as needed
            spacer.Format.SpaceAfter = Unit.FromCentimeter(0.2);


        }


        private void AddHeading(Section section, string text, string style, int fontSize = 16)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            Paragraph heading = section.AddParagraph();
            heading.Style = style;
            heading.Format.Font.Size = isFirst ? 20 : fontSize;
            heading.Format.Font.Bold = true;
            // Clean up the text (remove HTML tags and decode entities)
            string cleanText = WebUtility.HtmlDecode(StripHtml(text)).Trim();
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
                            AddTableToDocument(para, tableNode);
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

            // Calculate available width (page width - left/right margins)
            var pageSetup = section.PageSetup;
            double availableWidth = (pageSetup.PageWidth - pageSetup.LeftMargin - pageSetup.RightMargin).Centimeter;

            // Distribute columns evenly
            double columnWidth = availableWidth / columnCount;
            for (int i = 0; i < columnCount; i++)
            {
                table.AddColumn(Unit.FromCentimeter(columnWidth));
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
                                    AddImageToParagraph(paragraph, pContent);
                                }
                                else
                                {
                                    paragraph.AddText(WebUtility.HtmlDecode(pContent.InnerText));
                                }
                            }
                        }
                        else
                        {
                            paragraph.AddText(WebUtility.HtmlDecode(content.InnerText));
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

                            // Set width from HTML attributes
                            var widthAttr = imgNode.GetAttributeValue("width", "");
                            var styleWidth = imgNode.GetAttributeValue("style", "")
                                .Split(';')
                                .FirstOrDefault(s => s.Trim().StartsWith("width"))
                                ?.Split(':').LastOrDefault()?.Trim();

                            if (widthAttr == "293" || styleWidth == "293px")
                            {
                                image.Width = Unit.FromPoint(293);
                            }
                            else
                            {
                                image.Width = Unit.FromCentimeter(10); // Default width
                            }

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
        //    var rows = node.SelectNodes(".//tr");
        //    if (rows == null) return;

        //    bool isHeader = true;
        //    foreach (var rowNode in rows)
        //    {
        //        Row row = table.AddRow();

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

            // Add space before table (keep existing)
            Paragraph spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(0.5);

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            // Determine number of columns (keep existing)
            var firstRow = node.SelectSingleNode(".//tr");
            if (firstRow == null) return;

            var cells = firstRow.SelectNodes(".//th|.//td");
            if (cells == null || cells.Count == 0) return;

            // Add columns to the table (keep existing)
            double columnWidth = 16.0 / cells.Count;
            for (int i = 0; i < cells.Count; i++)
            {
                table.AddColumn(Unit.FromCentimeter(columnWidth));
            }

            // Process rows (keep existing structure)
            var rows = node.SelectNodes(".//tr");
            if (rows == null) return;

            bool isHeader = true;
            foreach (var rowNode in rows)
            {
                Row row = table.AddRow();

                // Process cells (keep existing structure)
                var cellNodes = rowNode.SelectNodes(".//th|.//td");
                if (cellNodes == null) continue;

                for (int i = 0; i < cellNodes.Count && i < table.Columns.Count; i++)
                {
                    var cellNode = cellNodes[i];
                    Cell cell = row.Cells[i];

                    // Apply header styling (keep existing)
                    if (isHeader || cellNode.Name.ToLower() == "th")
                    {
                        cell.Shading.Color = new Color(191, 191, 191);
                        cell.Format.Font.Bold = true;
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
            // First check if there are any images in this cell
            var images = cellNode.SelectNodes(".//img");

            if (images != null && images.Count > 0)
            {
                // Process each image
                foreach (HtmlNode imgNode in images)
                {
                    string imageUrl = imgNode.GetAttributeValue("src", "");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        try
                        {
                            // Clean URL if needed (remove query parameters)
                            Uri uri = new Uri(imageUrl);
                            string cleanUrl = uri.GetLeftPart(UriPartial.Path);

                            // Add image to cell
                            Paragraph imagePara = cell.AddParagraph();
                            MigraDoc.DocumentObjectModel.Shapes.Image image = imagePara.AddImage(cleanUrl);

                            // Set image dimensions if specified
                            string width = imgNode.GetAttributeValue("width", "");
                            string height = imgNode.GetAttributeValue("height", "");

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
                                    image.Height = Unit.FromCentimeter(pxHeight * 0.026458);
                                }
                            }

                            imagePara.Format.Alignment = ParagraphAlignment.Center;
                        }
                        catch
                        {
                            // Fallback to text if image fails
                            Paragraph para = cell.AddParagraph();
                            para.AddText($"[Image: {imageUrl}]");
                        }
                    }
                }
            }

            // Process all other content normally using your existing method
            Paragraph contentPara = cell.AddParagraph();
            ProcessInlineElements(contentPara, cellNode);
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
                Paragraph errorPara = section.AddParagraph();
                errorPara.AddText($"[Image: {src} - Failed to load]");
            }
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // Simple HTML tag removal
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }

    //public class HtmlRequestModel
    //{
    //    public string HtmlContent { get; set; }
    //}
}