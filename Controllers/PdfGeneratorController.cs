using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HtmlAgilityPack;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.Web;
using MigraDoc.DocumentObjectModel.Shapes;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http.Cors;
using System.Web.UI.WebControls.WebParts;
using iTextSharp.tool.xml.html;

//using iText.StyledXmlParser.Jsoup.Nodes;

namespace PickupAPi.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/PdfGenerator")]
    public class PdfGeneratorController : ApiController
    {
        private readonly List<(string Title, int PageNumber)> blockquoteIndex = new List<(string, int)>();

        [HttpPost]
        [Route("GeneratePdf")]
        public async Task<HttpResponseMessage> GeneratePdf([FromBody] HtmlRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.htmlContent))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid HTML Content");

            try
            {
                var title = request.title.Replace("&amp;", "&");
                var contentHtml = request.htmlContent.Replace("&amp;", "&");

                byte[] pdfBytes = GeneratePdfFromHtml(request.htmlContent, title);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(pdfBytes)
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "GeneratedDocument.pdf"
                };

                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.StackTrace);
            }
        }


        private void DefineStyles(Document doc)
        {
            // Normal text style
            Style normal = doc.Styles["Normal"];
            normal.Font.Name = "Arial";
            normal.Font.Size = 11;

            // Heading styles
            Style heading1 = doc.Styles["Heading1"];
            heading1.Font.Name = "Arial";
            heading1.Font.Size = 16;
            heading1.Font.Bold = true;
            heading1.Font.Color = new Color(28, 74, 113); // Nomadix blue color
            heading1.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.5);
            heading1.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.3);

            Style heading2 = doc.Styles["Heading2"];
            heading2.Font.Name = "Arial";
            heading2.Font.Size = 14;
            heading2.Font.Bold = true;
            heading2.Font.Color = new Color(28, 74, 113); // Nomadix blue color
            heading2.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.4);
            heading2.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

            Style heading3 = doc.Styles["Heading3"];
            heading3.Font.Name = "Arial";
            heading3.Font.Size = 12;
            heading3.Font.Bold = true;
            heading3.Font.Color = new Color(28, 74, 113); // Nomadix blue color
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

            Style warningStyle = doc.Styles.AddStyle("WarningBox", "Normal");
            warningStyle.ParagraphFormat.Borders.Width = 0.5;
            warningStyle.ParagraphFormat.Borders.Color = new Color(127, 100, 22); // Warning border color
            warningStyle.ParagraphFormat.Borders.Distance = 3;
            warningStyle.ParagraphFormat.Shading.Color = new Color(253, 242, 206); // Warning background color
            warningStyle.ParagraphFormat.LeftIndent = 10;
            warningStyle.ParagraphFormat.RightIndent = 10;
            warningStyle.Font.Color = new Color(127, 100, 22); // Warning text color

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


        private byte[] GeneratePdfFromHtml(string htmlContent, string title)
        {

            //Document doc = new Document();
            //DefineStyles(doc);

            //// Add cover page
            //Section coverSection = doc.AddSection();
            //AddCoverPage(doc);

            //// Add content section with header and footer
            //Section contentSection = doc.AddSection();
            //AddHeader(contentSection);
            //AddFooter(contentSection);



            Document doc = new Document();

            doc.Styles["Normal"].Font.Name = "Arial";
            doc.Styles["Normal"].Font.Size = 12; // Optional: Set default size
            doc.Styles["Normal"].Font.Color = Colors.Black; // Optional: Set default color

            // Add the cover page first
            AddCoverPage(doc, title);

            // Add an index page after the cover
            AddIndexPage(doc);

            // Add content
            //AddContentSections(doc);
            Section section = doc.AddSection();
            AddFooter(section);


            // Define page setup for content
            PageSetup pageSetup = section.PageSetup;
            //pageSetup.PageWidth = Unit.FromCentimeter(21);    // A4 width
            //pageSetup.PageHeight = Unit.FromCentimeter(29.7); // A4 height
            pageSetup.TopMargin = Unit.FromCentimeter(1.5);   // Reserve space for header
            pageSetup.BottomMargin = Unit.FromCentimeter(1.5); // Reserve space for footer
            //pageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            //pageSetup.RightMargin = Unit.FromCentimeter(2.5);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            //section.PageSetup = new PageSetup
            //{
            //    PageWidth = Unit.FromCentimeter(21), // Default A4 width
            //    PageHeight = Unit.FromCentimeter(29.7), // Default A4 height
            //    LeftMargin = Unit.FromCentimeter(2.5),
            //    RightMargin = Unit.FromCentimeter(2.5),
            //    TopMargin = Unit.FromCentimeter(2.5),
            //    BottomMargin = Unit.FromCentimeter(2.5)
            //};

            //AddHeader(section);
            //section.Footers.Primary.AddParagraph(); // Empty footer for other pages
            //section.Footers.EvenPage.AddParagraph(); // Empty footer for even pages


            foreach (HtmlNode node in htmlDoc.DocumentNode.ChildNodes)
            {
                AddSpace(section);
                ProcessNode(node, section, doc);
                //AddSpace(section);
                //section.Footers.Primary.AddParagraph(); // Empty footer for other pages
                //section.Footers.EvenPage.AddParagraph(); // Empty footer for even pages
            }

            ApplyDefaultFormatting(doc);

            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = doc;

            try
            {
                renderer.RenderDocument();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rendering document: {ex.StackTrace}");
                throw;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                renderer.PdfDocument.Save(stream, false);
                return stream.ToArray();
            }
        }


        private void AddHeader(Section section)
        {
            HeaderFooter footer = section.Footers.Primary;
            Table footerTable = footer.AddTable();

            // Full page width (A4 = 21 cm)
            double pageWidth = section.PageSetup.PageWidth.Centimeter;
            double leftMargin = section.PageSetup.LeftMargin.Centimeter;
            double rightMargin = section.PageSetup.RightMargin.Centimeter;

            // Calculate the usable width for the table
            double totalWidth = pageWidth; // Full width including margins

            // Add columns based on the full width
            Column footerCol1 = footerTable.AddColumn();
            footerCol1.Width = Unit.FromCentimeter(totalWidth * 0.70); // 70% width

            Column footerCol2 = footerTable.AddColumn();
            footerCol2.Width = Unit.FromCentimeter(totalWidth * 0.30); // 30% width

            // Add a row
            // Row footerRow = footerTable.AddRow();
            // footerRow.Shading.Color = new Color(122, 181, 92); // Light green background
            // footerRow.Height = Unit.FromCentimeter(1.2); // Reduced footer height
            // footerRow.Format.SpaceBefore = Unit.FromCentimeter(0.3); // Add space before footer
            // footerRow.Format.SpaceAfter = Unit.FromCentimeter(0.3); // Add space after footer

            // // Add left-aligned text
            // Paragraph leftFooterText = footerRow.Cells[0].AddParagraph();
            // leftFooterText.AddFormattedText("® NOMADIX Technical Assistance", TextFormat.Bold);
            // leftFooterText.Format.Font.Name = "Arial";
            // leftFooterText.Format.Font.Size = 11;
            // leftFooterText.Format.Font.Color = Colors.White;
            // leftFooterText.Format.Alignment = ParagraphAlignment.Left;

            // // Add right-aligned text
            // Paragraph rightFooterText = footerRow.Cells[1].AddParagraph();
            // rightFooterText.AddFormattedText("Nomadix Nexus Portal", TextFormat.Bold);
            // rightFooterText.Format.Font.Name = "Arial";
            // rightFooterText.Format.Font.Size = 11;
            // rightFooterText.Format.Font.Color = Colors.White;
            // rightFooterText.Format.Alignment = ParagraphAlignment.Right;

        }

        //private void AddHeader(Section section)
        //{
        //    // HEADER SETUP
        //    Table headerTable = section.Headers.Primary.AddTable();

        //    // Set the table width to the full page width
        //    double pageWidth = section.PageSetup.PageWidth.Centimeter;

        //    // Add columns based on the full width
        //    headerTable.AddColumn(Unit.FromCentimeter(pageWidth * 0.30)); // Logo column
        //    headerTable.AddColumn(Unit.FromCentimeter(pageWidth * 0.70)); // Info column

        //    // Add a row
        //    Row headerRow = headerTable.AddRow();
        //    headerRow.VerticalAlignment = VerticalAlignment.Center;

        //    // Add logo to the header
        //    string logoUrl = @"https://cdn.document360.io/logo/456bc41e-6bf6-4ebf-a659-426e82b994f7/8cea39fb9ddf47dcbd9937cd625834b0-nomadixlogo.png";
        //    try
        //    {
        //        using (var client = new WebClient())
        //        {
        //            byte[] logoData = client.DownloadData(logoUrl);
        //            string tempLogoPath = Path.GetTempFileName();
        //            File.WriteAllBytes(tempLogoPath, logoData);

        //            var logoImage = headerRow.Cells[0].AddImage(tempLogoPath);
        //            logoImage.LockAspectRatio = true;
        //            logoImage.Width = Unit.FromCentimeter(4);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error loading logo: {ex.Message}");
        //    }

        //    // Add header text
        //    Paragraph headerText = headerRow.Cells[1].AddParagraph();
        //    headerText.AddFormattedText("Nomadix Nexus Portal", TextFormat.Bold);
        //    headerText.Format.Font.Size = 16;
        //    headerText.Format.Font.Name = "Arial";
        //    headerText.Format.Alignment = ParagraphAlignment.Right;

        //    //// Remove margin effects
        //    //headerTable.Rows.LeftIndent = -section.PageSetup.LeftMargin.Centimeter;

        //    //AddFooter(section);
        //}

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
            copyrightParagraph.Format.Font.Size = 9;
            copyrightParagraph.Format.Alignment = ParagraphAlignment.Left;
            copyrightParagraph.Format.LeftIndent = Unit.FromCentimeter(1.8);
            

            // Add page number to the right
            Paragraph pageNumberParagraph = footerRow.Cells[1].AddParagraph();
            pageNumberParagraph.AddText("Page ");
            pageNumberParagraph.AddPageField();
            pageNumberParagraph.AddText(" of ");
            pageNumberParagraph.AddNumPagesField();
            pageNumberParagraph.Format.Font.Size = 9;
            pageNumberParagraph.Format.Alignment = ParagraphAlignment.Right;

            // // Add a horizontal line
            // Paragraph line = footer.AddParagraph();
            // line.Format.Borders.Top.Width = 0.75;
            // line.Format.Borders.Top.Color = new Color(28, 74, 113); // Nomadix blue color
        }

        //private void ProcessNode(HtmlNode node, Section section)
        //{

        //    switch (node.Name.ToLower())
        //    {
        //        case "h1":
        //            Paragraph h1 = section.AddParagraph(node.InnerText.Trim());
        //            h1.Format.Font.Size = 18;
        //            h1.Format.Font.Bold = true;
        //            h1.Format.SpaceAfter = "0.5cm";
        //            h1.Format.Alignment = ParagraphAlignment.Left; // Align to the left
        //            break;

        //        case "h2":
        //            Paragraph h2 = section.AddParagraph(node.InnerText.Trim());
        //            h2.Format.Font.Size = 16;
        //            h2.Format.Font.Bold = true;
        //            h2.Format.SpaceAfter = "0.4cm";
        //            h2.Format.Alignment = ParagraphAlignment.Left;
        //            break;

        //        case "h3":
        //            Paragraph h3 = section.AddParagraph(node.InnerText.Trim());
        //            h3.Format.Font.Size = 14;
        //            h3.Format.Font.Bold = true;
        //            h3.Format.SpaceAfter = "0.3cm";
        //            break;

        //        case "p":
        //            // Loop through all child nodes inside <p>
        //            foreach (var childNode in node.ChildNodes)
        //            {
        //                switch (childNode.Name.ToLower())
        //                {
        //                    // Handle <img> tag inside <p>
        //                    case "img":
        //                        string imageUrl = childNode.GetAttributeValue("src", string.Empty);
        //                        imageUrl = HttpUtility.HtmlDecode(imageUrl);
        //                        if (!string.IsNullOrEmpty(imageUrl))
        //                        {
        //                            AddImageToSection(imageUrl, section);
        //                        }
        //                        break;

        //                    case "video":
        //                        string videoUrl = childNode.GetAttributeValue("src", string.Empty);
        //                        if (!string.IsNullOrEmpty(videoUrl))
        //                        {
        //                            AddVideoToSection(videoUrl, section);
        //                        }
        //                        break;

        //                    // Handle table inside <p> tag (if there is a <table> inside <p>)
        //                    case "table":
        //                        // Table processing logic (similar to previous examples)
        //                        //ProcessTable(childNode, section);
        //                        if (node.Name == "table")
        //                        {
        //                            ProcessTable(node, section);
        //                        }

        //                        break;

        //                    // Handle headers (h1, h2, h3, etc.)
        //                    case "h1":
        //                    case "h2":
        //                    case "h3":
        //                    case "h4":
        //                    case "h5":
        //                    case "h6":
        //                        string headerText = childNode.InnerText.Trim();
        //                        //section.AddParagraph(headerText).Format.Font.Size = 16; // Adjust size as needed
        //                        AddHeading(node.InnerText.Trim(), section, 16);
        //                        break;

        //                    // Handle text nodes directly (any other content like normal text)
        //                    case "#text":
        //                        string paragraphText = childNode.InnerText.Trim();
        //                        if (!string.IsNullOrEmpty(paragraphText))
        //                        {
        //                            section.AddParagraph(paragraphText);
        //                        }
        //                        break;
        //                }
        //            }
        //            break;

        //        case "blockquote":

        //            string type = "";
        //            if (node.InnerText.Trim().ToString().IndexOf("Note") > 0)
        //            {
        //                type = HttpContext.Current.Server.MapPath("~/logo/info_icon.png");
        //                AddSpace(section);
        //                AddStyledSection(section, "Note", node.InnerText.Trim(), Colors.DarkBlue, Colors.White, type);
        //            }
        //            else if (node.InnerText.Trim().ToString().IndexOf("Warning") > 0)
        //            {

        //                type = HttpContext.Current.Server.MapPath("~/logo/warning_icon.png");
        //                AddStyledSection(section, "Warning", node.InnerText.Trim(), Colors.DarkGoldenrod, Colors.White, type);
        //            }


        //            // Add the blockquote title to the index list
        //            blockquoteIndex.Add((node.InnerText.Trim(), 0)); // Page number will be updated later
        //            break;

        //        case "ul":
        //        case "ol":
        //            var listItems = node.SelectNodes("li");
        //            if (listItems != null)
        //            {
        //                foreach (HtmlNode li in listItems)
        //                {
        //                    //Paragraph listItem = section.AddParagraph($"• {li.InnerText.Trim()}");
        //                    //listItem.Format.Font.Size = 12;
        //                    //listItem.Format.LeftIndent = Unit.FromCentimeter(0.5);

        //                    // Recursively process all child nodes inside <li>
        //                    ProcessListItemChildren(li, section);
        //                }
        //            }

        //            break;

        //        case "table":
        //            ProcessTable(node, section);
        //            break;

        //        //case "img":
        //        //    string imgUrl = node.GetAttributeValue("src", "");
        //        //    if (!string.IsNullOrEmpty(imgUrl))
        //        //    {
        //        //        try
        //        //        {
        //        //            AddImageToSection(imgUrl, section);
        //        //        }
        //        //        catch { /* Ignore invalid images */ }
        //        //    }
        //        //    break;

        //        //case "video":
        //        //    AddVideoToSection
        //        //    break;

        //        default:
        //            foreach (HtmlNode child in node.ChildNodes)
        //            {
        //                ProcessNode(child, section);
        //            }
        //            break;
        //    }
        //}


        private void ProcessNode(HtmlNode node, Section section, Document doc)
        {

            switch (node.Name.ToLower())
            {
                case "h1":
                    AddHeading(node.InnerText.Trim(), section, 24);
                    break;

                case "h2":
                    AddHeading(node.InnerText.Trim(), section, 20);
                    break;

                case "h3":
                    AddHeading(node.InnerText.Trim(), section, 18);
                    break;

                case "p":
                    // Process text and nested elements inside <p>
                    foreach (var childNode in node.ChildNodes)
                    {
                        switch (childNode.Name.ToLower())
                        {
                            case "iframe":
                                ProcessYouTubeVideo(childNode, section);
                                break;
                            case "img":
                                string imageUrl = childNode.GetAttributeValue("src", string.Empty);
                                imageUrl = HttpUtility.HtmlDecode(imageUrl);
                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    AddImageToSection(imageUrl, section);
                                }
                                break;

                            case "#text":
                                string text = childNode.InnerText.Trim();
                                if (!string.IsNullOrEmpty(text))
                                {
                                    Paragraph p = section.AddParagraph(text);
                                    p.Format.Font.Size = 12;
                                    p.Format.LeftIndent = Unit.FromCentimeter(2);
                                    p.Format.RightIndent = Unit.FromCentimeter(2);
                                }
                                break;

                            default:
                                ProcessNode(childNode, section, doc); // Recursively process unknown elements
                                break;
                        }
                    }
                    break;

                case "blockquote":
                    string type = "";
                    if (node.InnerText.Trim().ToString().IndexOf("Note") > 0)
                    {
                        type = HttpContext.Current.Server.MapPath("~/logo/info_icon.png");
                        AddSpace(section);
                        AddStyledSection(section, "Note", node.InnerText.Trim(), Colors.DarkBlue, Colors.White, type);
                    }
                    else if (node.InnerText.Trim().ToString().IndexOf("Warning") > 0)
                    {

                        type = HttpContext.Current.Server.MapPath("~/logo/warning_icon.png");
                        AddStyledSection(section, "Warning", node.InnerText.Trim(), Colors.DarkGoldenrod, Colors.White, type);
                    }
                    // Add the blockquote title to the index list
                    blockquoteIndex.Add((node.InnerText.Trim(), 0)); // Page number will be updated later                    
                    break;

                case "ul":
                case "ol":
                    // Only process <li> children of the list
                    var listItems = node.SelectNodes("li");
                    if (listItems != null)
                    {
                        foreach (var li in listItems)
                        {
                            ProcessListItemChildren(li, section);
                        }
                    }
                    break;

                case "li":
                    // Avoid redundant handling of <li> directly here; rely on the ul/ol handler.
                    break;

                case "table":
                    ProcessTable(node, section);
                    break;

                default:
                    // Recursively process any other child nodes
                    foreach (HtmlNode child in node.ChildNodes)
                    {
                        ProcessNode(child, section, doc);
                    }
                    break;
            }
        }





        private void ProcessListItemChildren(HtmlNode parentNode, Section section)
        {
            foreach (var childNode in parentNode.ChildNodes)
            {
                // Handle div containers
                if (childNode.Name.ToLower() == "div")
                {
                    foreach (var divChild in childNode.ChildNodes)
                    {
                        ProcessListItemChildren(divChild, section);
                    }
                    continue;
                }

                // Handle table elements
                if (childNode.Name.ToLower() == "table")
                {
                    var imgNodes = childNode.SelectNodes("//img");
                    if (imgNodes != null)
                    {
                        foreach (var imgNode in imgNodes)
                        {
                            string imageUrl = imgNode.GetAttributeValue("src", string.Empty);
                            imageUrl = HttpUtility.HtmlDecode(imageUrl);
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                AddImageToSection(imageUrl, section);
                            }
                        }
                    }
                    continue;
                }
                switch (childNode.Name.ToLower())
                {
                    case "p":
                        Paragraph p = section.AddParagraph($"• {childNode.InnerText.Trim().Trim()}");
                        p.Format.Font.Size = 12;
                        p.Format.LeftIndent = Unit.FromCentimeter(2.5);
                        p.Format.RightIndent = Unit.FromCentimeter(2.5);

                        // Check for images inside <p>
                        foreach (var nestedChild in childNode.ChildNodes)
                        {
                            if (nestedChild.Name.ToLower() == "img")
                            {
                                string imageUrl = nestedChild.GetAttributeValue("src", string.Empty);
                                imageUrl = HttpUtility.HtmlDecode(imageUrl);
                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    AddImageToSection(imageUrl, section);
                                }
                            }
                        }
                        break;

                    case "img": // Handle direct images
                        string imgUrl = childNode.GetAttributeValue("src", string.Empty);
                        imgUrl = HttpUtility.HtmlDecode(imgUrl)?.Trim();
                        if (!string.IsNullOrEmpty(imgUrl))
                        {
                            imgUrl = imgUrl.Replace(" ", ""); // Remove any spaces in URL
                            AddImageToSection(imgUrl, section);
                        }
                        break;

                    case "hr": // Handle horizontal rule, if needed
                        //section.AddParagraph("______________________"); // Simulating an HR divider
                        Paragraph pg = section.AddParagraph();
                        pg.Format.Borders.Bottom.Width = 1;

                        break;

                    case "h2":
                    case "h3":
                    case "h4":
                        Paragraph header = section.AddParagraph(childNode.InnerText.Trim());
                        header.Format.Font.Size = 14;
                        header.Format.Font.Bold = true;
                        header.Format.SpaceBefore = Unit.FromCentimeter(1);

                        header.Format.LeftIndent = Unit.FromCentimeter(2);
                        header.Format.RightIndent = Unit.FromCentimeter(2);

                        break;

                        //default:
                        //    // If there are nested elements inside <li>, process them recursively
                        //    ProcessListItemChildren(childNode, section);
                        //    break;
                }
            }
        }

        private void ProcessList(HtmlNode listNode, Section section, int indentLevel = 0)
        {
            if (listNode == null)
                return;

            var listItems = listNode.SelectNodes("li");
            if (listItems == null)
                return;

            foreach (HtmlNode li in listItems)
            {
                // Extract the text content of the <li>
                string listItemText = li.InnerText.Trim();
                if (!string.IsNullOrEmpty(listItemText))
                {
                    Paragraph listItemParagraph = section.AddParagraph($"• {listItemText}");
                    listItemParagraph.Format.Font.Size = 12;
                    listItemParagraph.Format.LeftIndent = Unit.FromCentimeter(4.5); // Increase indent for nested lists
                    listItemParagraph.Format.RightIndent = Unit.FromCentimeter(4.5); // Increase indent for nested lists
                }

                // Process child elements inside <li>
                foreach (HtmlNode childNode in li.ChildNodes)
                {
                    switch (childNode.Name.ToLower())
                    {
                        case "p":
                            ProcessParagraph(childNode, section);
                            break;

                        case "img":
                            string imageUrl = childNode.GetAttributeValue("src", string.Empty);
                            imageUrl = HttpUtility.HtmlDecode(imageUrl);
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                AddImageToSection(imageUrl, section);
                            }
                            break;

                        case "ul":
                        case "ol":
                            // Recursively process nested lists, increasing the indent
                            ProcessList(childNode, section, indentLevel + 1);
                            break;
                    }
                }
            }
        }


        private void ProcessParagraph(HtmlNode node, Section section)
        {
            if (node == null)
                return;

            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = 12;
            paragraph.Format.LeftIndent = Unit.FromCentimeter(2);
            paragraph.Format.RightIndent = Unit.FromCentimeter(2);
            foreach (HtmlNode child in node.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "#text":  // Extracts text inside the paragraph
                        paragraph.AddText(child.InnerText.Trim());
                        paragraph.Format.LeftIndent = Unit.FromCentimeter(4.5); // Increase indent for nested lists
                        paragraph.Format.RightIndent = Unit.FromCentimeter(4.5); // Increase indent for nested lists
                        break;

                    case "strong": // Handles bold text inside <p>
                    case "b": // Also handle <b> tags the same way
                        paragraph.AddFormattedText(child.InnerText.Trim(), TextFormat.Bold);
                        paragraph.Format.LeftIndent = Unit.FromCentimeter(4.5); // Increase indent for nested lists
                        paragraph.Format.RightIndent = Unit.FromCentimeter(4.5); // Increase indent for nested lists
                        break;

                    case "img":  // Handles inline images inside <p>
                        string imageUrl = child.GetAttributeValue("src", string.Empty);
                        imageUrl = HttpUtility.HtmlDecode(imageUrl);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            AddImageToSection(imageUrl, section);
                        }
                        break;
                }
            }
        }


        private void AddStyledSection(Section section, string title, string content, Color backgroundColor, Color textColor, string iconPath = null)
        {
            // Create a table to structure the section
            Table table = section.AddTable();
            table.Borders.Width = 0; // No border for the table

            // Add columns: one for the icon, one for the text content
            table.AddColumn(Unit.FromCentimeter(2)); // Icon column
            table.AddColumn(Unit.FromCentimeter(14)); // Text content column
            table.Rows.LeftIndent = Unit.FromCentimeter(2); // Indent the table
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
                    iconImage.Width = Unit.FromCentimeter(0.8); // Adjust the width
                    iconImage.Height = Unit.FromCentimeter(0.8); // Adjust the height
                    iconImage.Left = ShapePosition.Right; // Center the image horizontally
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
            contentParagraph.AddText(updatedContent);
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

        //private void AddStyledSection(Section section, string title, string content, Color backgroundColor, Color textColor, string iconPath = null)
        //{
        //    // Create a paragraph for the section
        //    Paragraph paragraph = section.AddParagraph();

        //    // Apply background color (using shading if supported)
        //    paragraph.Format.Shading.Color = backgroundColor;

        //    // Add the icon if available
        //    if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
        //    {
        //        try
        //        {
        //            var iconImage = paragraph.AddImage(iconPath);
        //            iconImage.LockAspectRatio = true; // Maintain aspect ratio
        //            iconImage.Width = Unit.FromCentimeter(1); // Adjust width
        //            iconImage.Height = Unit.FromCentimeter(1); // Adjust height
        //            iconImage.Left = ShapePosition.Left; // Align left
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error adding icon image: {ex.Message}");
        //        }
        //    }

        //    // Add the title (bold and larger font)
        //    paragraph.AddFormattedText(title + "\n", TextFormat.Bold);
        //    paragraph.Format.Font.Color = textColor;
        //    paragraph.Format.Font.Size = 11; // Title font size
        //    paragraph.Format.Font.Name = "Arial";
        //    paragraph.Format.Alignment = ParagraphAlignment.Left;
        //    // Add the content
        //    string updatedContent = content.Replace("\u2139\uFE0F", "") // Replace ℹ️
        //                                   .Replace("\u26A0\uFE0F", ""); // Replace ⚠️
        //    paragraph.AddText(updatedContent);
        //    paragraph.Format.Font.Color = textColor;
        //    paragraph.Format.Font.Size = 9; // Content font size
        //    paragraph.Format.Font.Name = "Arial";
        //    paragraph.Format.Alignment = ParagraphAlignment.Right;

        //    // Add spacing after paragraph
        //    paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);
        //    paragraph.Format.LeftIndent = Unit.FromCentimeter(2);
        //    paragraph.Format.RightIndent = Unit.FromCentimeter(2);


        //}


        private void FormatTable(Table table)
        {
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Black;

            foreach (Row row in table.Rows)
            {
                foreach (Cell cell in row.Cells)
                {
                    cell.Format.Font.Name = "Arial";
                    cell.Format.Font.Size = 12;
                    cell.Format.Alignment = ParagraphAlignment.Left;

                    cell.Borders.Left.Width = 0.5;
                    cell.Borders.Right.Width = 0.5;
                    cell.Borders.Top.Width = 0.5;
                    cell.Borders.Bottom.Width = 0.5;



                    cell.VerticalAlignment = VerticalAlignment.Center;
                    cell.Format.SpaceBefore = Unit.FromPoint(3); // Add padding
                    cell.Format.SpaceAfter = Unit.FromPoint(3);
                }
            }
            table.Format.SpaceBefore = Unit.FromCentimeter(1.2);
            table.Format.SpaceAfter = Unit.FromCentimeter(2);

        }

        private void ApplyDefaultFormatting(Document doc)
        {
            // Set global font for the document
            //doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            //doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(2.5);
            //doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(2.5);
            //doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(2.5);

            foreach (Section section in doc.Sections)
            {
                foreach (Paragraph paragraph in section.Elements.OfType<Paragraph>())
                {
                    paragraph.Format.Font.Name = "Arial";
                    paragraph.Format.Font.Size = 11; // Set a uniform font size
                    paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);

                }
            }
        }

        private void AddCoverPage(Document doc, string title)
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


            // // Add the left-aligned text
            section.AddParagraph().AddText("title");
            



        }

        private void AddCoverPage_old(Document doc)
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
            // Table footerTable = section.Footers.Primary.AddTable();
            // footerTable.Borders.Width = 0;
            // footerTable.AddColumn(Unit.FromCentimeter(16)); // Text column
            // footerTable.AddColumn(Unit.FromCentimeter(5));  // Logo column


            // Row footerRow = footerTable.AddRow();

            // // Set the background color
            // footerRow.Shading.Color = new Color(122, 181, 92); // Light green background

            // // Add the left-aligned text
            // Paragraph leftText = footerRow.Cells[0].AddParagraph();
            // leftText.AddFormattedText("® NOMADIX", TextFormat.Bold);
            // leftText.Format.Font.Name = "Arial";
            // leftText.Format.Font.Size = 11;
            // leftText.Format.Font.Color = Colors.White;
            // leftText.Format.Alignment = ParagraphAlignment.Left;
            // leftText.Format.LeftIndent = Unit.FromCentimeter(0.5); // Adjust left indent

            // // Set vertical alignment for the left cell
            // footerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;



            // // Adjust the height of the row to fit content
            // footerRow.Height = Unit.FromCentimeter(1.2); // Reduced footer height
            // footerRow.Format.SpaceBefore = Unit.FromCentimeter(0.3); // Add space before footer
            // footerRow.Format.SpaceAfter = Unit.FromCentimeter(0.3); // Add space after footer



        }


        private void AddIndexPage(Document doc)
        {
            Section indexSection = doc.AddSection();
            indexSection.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21), // Default A4 width
                PageHeight = Unit.FromCentimeter(29.7), // Default A4 height
                LeftMargin = Unit.FromCentimeter(0),
                RightMargin = Unit.FromCentimeter(0),
                TopMargin = Unit.FromCentimeter(0),
                BottomMargin = Unit.FromCentimeter(0)
            };

            //indexSection.PageSetup.LeftMargin = Unit.FromCentimeter(2); // Standard left margin for content
            //indexSection.PageSetup.RightMargin = Unit.FromCentimeter(2); // Standard right margin for content

            // Index title
            Paragraph title = indexSection.AddParagraph("Table of Contents");
            title.Format.Font.Size = 20;
            title.Format.Font.Bold = true;
            title.Format.Font.Name = "Arial";
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = Unit.FromCentimeter(1);
            //title.Format.LeftIndent = Unit.FromPoint(5);
            //title.Format.RightIndent = Unit.FromPoint(5);

            // Sample Index Entries
            string[] indexItems = {
            "About our project objective(s)...................................................................................3",
            "About Userflow & Document360.........................................................................4",
            "Userflow Overview.................................................................................................5",
            "Document360 Overview.......................................................................................6",
            "Security capabilities................................................................................................7",
            "How Userflow and Document360 integrate with our products...........................8",
            "Target Architecture..................................................................................................9"
        };

            foreach (var item in indexItems)
            {
                Paragraph indexEntry = indexSection.AddParagraph(item);
                indexEntry.Format.Font.Size = 12;
                indexEntry.Format.Font.Name = "Arial";
                indexEntry.Format.SpaceAfter = Unit.FromCentimeter(0.5);

                indexEntry.Format.LeftIndent = Unit.FromCentimeter(2);
                indexEntry.Format.RightIndent = Unit.FromCentimeter(2);
            }
        }

        private void AddIndexPage2(Document doc)
        {
            Section indexSection = doc.AddSection();
            indexSection.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21), // Default A4 width
                PageHeight = Unit.FromCentimeter(29.7), // Default A4 height
                LeftMargin = Unit.FromCentimeter(2.5),
                RightMargin = Unit.FromCentimeter(2.5),
                TopMargin = Unit.FromCentimeter(2.5),
                BottomMargin = Unit.FromCentimeter(2.5)
            };

            // Add index title
            Paragraph title = indexSection.AddParagraph("Table of Contents");
            title.Format.Font.Size = 20;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = Unit.FromCentimeter(1);

            // Add blockquote titles with page numbers
            foreach (var (blockTitle, pageNumber) in blockquoteIndex.OrderBy(i => i.PageNumber))
            {
                string entry = $"{blockTitle}.........................................................{pageNumber}";
                Paragraph indexEntry = indexSection.AddParagraph(entry);
                indexEntry.Format.Font.Size = 12;
                indexEntry.Format.SpaceAfter = Unit.FromCentimeter(0.5);
            }
        }

        private void AddHeading(string text, Section section, int fontSize)
        {
            Paragraph para = section.AddParagraph(text);
            para.Format.Font.Size = fontSize;
            para.Format.Font.Bold = true;
            //para.Format.SpaceAfter = Unit.FromCentimeter(2);
            //para.Format.SpaceBefore = Unit.FromCentimeter(2);

            para.Format.LeftIndent = Unit.FromCentimeter(2);
            para.Format.RightIndent = Unit.FromCentimeter(2);
        }

        private void AddImageToSection(string imageUrl, Section section)
        {
            try
            {
                // Decode the URL to ensure any special characters are handled properly
                string decodedUrl = HttpUtility.HtmlDecode(imageUrl);

                // Generate a unique filename to avoid conflicts
                string fileName = Guid.NewGuid().ToString() + ".png";
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
                paragraph.Format.LeftIndent = Unit.FromCentimeter(2);
                paragraph.Format.RightIndent = Unit.FromCentimeter(2);
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

        private void ProcessTable(HtmlNode node, Section section)
        {

            try
            {
                //Paragraph wrapper = section.AddParagraph();
                //wrapper.Format.Alignment = ParagraphAlignment.Center;
                //wrapper.Format.SpaceAfter = Unit.FromCentimeter(1);
                //wrapper.Format.LeftIndent = Unit.FromCentimeter(2);
                //wrapper.Format.RightIndent = Unit.FromCentimeter(2);


                Table table = new Table();
                table.Borders.Width = 0.75;

                // Add left and right indents to the table
                //table.Format.LeftIndent = Unit.FromCentimeter(2);  // Add left margin
                //table.Format.RightIndent = Unit.FromCentimeter(2); // Add right margin
                table.Rows.LeftIndent = Unit.FromCentimeter(2);    // Align rows to the left margin

                // Handle colgroup (column widths)
                var colgroup = node.SelectSingleNode("colgroup");
                if (colgroup != null)
                {
                    var cols = colgroup.SelectNodes("col");
                    double totalWidth = 480; // Adjust total table width as per your requirements (e.g., page width minus margins)

                    if (cols.Count == 2)
                    {
                        table.AddColumn(Unit.FromPoint(totalWidth * 0.3)); // 30% for the first column
                        table.AddColumn(Unit.FromPoint(totalWidth * 0.7)); // 70% for the second column
                    }
                    else if (cols.Count == 3)
                    {
                        table.AddColumn(Unit.FromPoint(totalWidth * 0.3)); // 30% for the first column
                        table.AddColumn(Unit.FromPoint(totalWidth * 0.3)); // 30% for the second column
                        table.AddColumn(Unit.FromPoint(totalWidth * 0.4)); // 40% for the third column
                    }
                    else
                    {
                        // Equal distribution for other column counts
                        double columnWidth = totalWidth / cols.Count;
                        for (int i = 0; i < cols.Count; i++)
                        {
                            table.AddColumn(Unit.FromPoint(columnWidth));
                        }
                    }
                }

                // Process rows
                var rows = node.SelectNodes("tbody/tr");
                if (rows != null)
                {
                    foreach (HtmlNode row in rows)
                    {
                        Row newRow = table.AddRow();

                        var cells = row.SelectNodes("th|td");
                        if (cells != null)
                        {
                            int columnIndex = 0;
                            foreach (HtmlNode cell in cells)
                            {
                                int colspan = cell.GetAttributeValue("colspan", 1);
                                int rowspan = cell.GetAttributeValue("rowspan", 1);

                                // Handle colspan (expand cells across columns)
                                for (int i = 0; i < colspan; i++)
                                {
                                    // Add a cell to the row for each column spanned
                                    if (columnIndex >= newRow.Cells.Count)
                                    {
                                        newRow.Cells.Add(new Cell());
                                    }

                                    // Set content for the cell
                                    Cell tableCell = newRow.Cells[columnIndex++];
                                    tableCell.AddParagraph(cell.InnerText.Trim());

                                    // Apply styles to header cells
                                    if (cell.Name == "th")
                                    {
                                        tableCell.Format.Font.Bold = true;
                                        tableCell.Shading.Color = new MigraDoc.DocumentObjectModel.Color((byte)0x00, (byte)0x2F, (byte)0x72); // Hex: #002F72
                                        tableCell.Format.Font.Color = Colors.White;
                                        tableCell.Format.Font.Name = "Arial";
                                        //var paragraph = tableCell.AddParagraph(cell.InnerText.Trim());
                                        //tableCell.Format.SpaceBefore = Unit.FromPoint(5); // Simulate top padding
                                        //tableCell.Format.SpaceAfter = Unit.FromPoint(5);  // Simulate bottom padding
                                        //tableCell.Format.LeftIndent = Unit.FromPoint(1);  // Simulate left padding
                                        //tableCell.Format.RightIndent = Unit.FromPoint(1); // Simulate right padding

                                        tableCell.Borders.Left.Width = 1;  // Adjust left border width
                                        tableCell.Borders.Top.Width = 1;  // Adjust top border width
                                        tableCell.Borders.Right.Width = 1; // Adjust right border width
                                        tableCell.Borders.Bottom.Width = 1; // Adjust bottom border width


                                    }
                                }
                            }
                        }
                    }
                }

                FormatTable(table);
                section.Add(table);
                Paragraph spacer = section.AddParagraph();
                spacer.Format.SpaceBefore = Unit.FromCentimeter(0.3); // Adjust spacing as needed
                spacer.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void ProcessYouTubeVideo(HtmlNode node, Section section)
        {

            // Get the YouTube video link from the iframe's src attribute
            string videoUrl = node.GetAttributeValue("src", string.Empty);

            // Ensure the URL is valid
            if (!string.IsNullOrEmpty(videoUrl))
            {
                // Convert YouTube embed link to a regular YouTube link (if necessary)
                if (videoUrl.Contains("youtube.com/embed/"))
                {
                    videoUrl = videoUrl.Replace("youtube.com/embed/", "youtube.com/watch?v=");
                }

                // Add a paragraph to the section for the hyperlink
                Paragraph videoParagraph = section.AddParagraph();
                videoParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.5); // Add some space before
                videoParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);  // Add some space after
                videoParagraph.Format.Font.Size = 12;                         // Adjust font size if needed
                videoParagraph.Format.Font.Color = Colors.Blue;               // Make it look like a link
                videoParagraph.Format.Font.Underline = Underline.Single;      // Underline for link appearance
                videoParagraph.Format.LeftIndent = Unit.FromCentimeter(2);    // Indent the hyperlink
                videoParagraph.Format.RightIndent = Unit.FromCentimeter(2);    // Indent the hyperlink
                // Add the clickable hyperlink
                Hyperlink hyperlink = videoParagraph.AddHyperlink(videoUrl, HyperlinkType.Web);
                hyperlink.AddText("Watch Video on YouTube");

                // Optional: Add a description below the link
                Paragraph descriptionParagraph = section.AddParagraph();
                descriptionParagraph.Format.SpaceBefore = Unit.FromPoint(5);
                descriptionParagraph.AddFormattedText("Click the link above to watch the video on YouTube.", TextFormat.Italic);
                descriptionParagraph.Format.LeftIndent = Unit.FromCentimeter(2);    // Indent the hyperlink
                descriptionParagraph.Format.RightIndent = Unit.FromCentimeter(2);    // Indent the hyperlink
            }
        }


        private void AddVideoToSection(string videoUrl, Section section)
        {
            try
            {
                if (string.IsNullOrEmpty(videoUrl))
                    return;

                // Video Placeholder
                Paragraph videoParagraph = section.AddParagraph("Video:");
                videoParagraph.Format.Font.Size = 12;
                videoParagraph.Format.Font.Color = Colors.Blue;
                videoParagraph.Format.Font.Name = "Arial";

                Hyperlink videoLink = videoParagraph.AddHyperlink(videoUrl, HyperlinkType.Web);
                videoLink.AddFormattedText(" Click Here", TextFormat.Bold);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding video: {ex.StackTrace}");
            }
        }







        private void AddSpace(Section section)
        {
            // Add a blank paragraph to create space after the section
            Paragraph spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(2); // Adjust spacing as needed
            spacer.Format.SpaceAfter = Unit.FromCentimeter(2);
        }
    }

    public class HtmlRequestModel
    {
        public string htmlContent { get; set; }
        public string title { get; set; }

        public int CoverPageType { get; set; }
        }
}