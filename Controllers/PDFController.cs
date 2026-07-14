using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace PickupAPi.Controllers
{
    [EnableCors(origins: "https://nomadix-kms.document360.io", headers: "*", methods: "*")]
    [RoutePrefix("api/Pdf")]
    public class PdfController : ApiController
    {
        private readonly List<(string Title, int PageNumber)> blockquoteIndex = new List<(string, int)>();


        [HttpGet]
        [Route("api/pdf/section")]
        public async Task<HttpResponseMessage> DownloadSection(string articleId)
        {
            var service = new Document360Service();

            // 1. Get current article
            var article = await service.GetArticle(articleId);

            // 2. Get category (parent = 1)
            var categoryId = article.CategoryId;

            // 3. Get all articles under that category
            var articles = await service.GetArticlesByCategory(categoryId);

            // 4. Sort (important)
            articles = articles.OrderBy(a => a.Title).ToList();

            // 5. Merge HTML
            var html = BuildMergedHtml(articles);

            // 6. Generate PDF
            byte[] pdfBytes = PdfGenerator.Generate(html);

            // 7. Return response
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(pdfBytes);
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/pdf");

            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"Section_{categoryId}.pdf"
                };

            return response;
        }

        private string BuildMergedHtml(List<Article> articles)
        {
            var sb = new StringBuilder();

            sb.Append("<html><body>");

            foreach (var article in articles)
            {
                sb.Append($"<h1>{article.Title}</h1>");
                sb.Append(article.HtmlContent);

                // Page break
                sb.Append("<div style='page-break-after: always;'></div>");
            }

            sb.Append("</body></html>");

            return sb.ToString();
        }

        [HttpPost]
        [Route("GeneratePdf")]
        public async Task<HttpResponseMessage> GeneratePdf([FromBody] HtmlRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.htmlContent))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid HTML Content");

            try
            {

                var contentHtml = request.htmlContent.Replace("&amp;", "&");



                byte[] pdfBytes = GeneratePdf(request.htmlContent);

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

        public byte[] GeneratePdf(string htmlContent)
        {
            Document doc = new Document();
            DefineStyles(doc);

            // Add header and footer
            Section section = doc.AddSection();
            //AddHeader(section);
            //AddFooter(section);

            // Define page setup for content only (not header/footer)
            PageSetup pageSetup = doc.DefaultPageSetup;
            pageSetup.PageWidth = Unit.FromCentimeter(21);    // A4 width
            pageSetup.PageHeight = Unit.FromCentimeter(29.7); // A4 height
            pageSetup.TopMargin = Unit.FromCentimeter(3);     // Reserve space for header
            pageSetup.BottomMargin = Unit.FromCentimeter(2);  // Reserve space for footer
            pageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            pageSetup.RightMargin = Unit.FromCentimeter(2.5);
            //section.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            //section.PageSetup.RightMargin = Unit.FromCentimeter(2.5);
            //section.PageSetup.TopMargin = Unit.FromCentimeter(4.5);
            //section.PageSetup.BottomMargin = Unit.FromCentimeter(2.5);

            // Process HTML content
            //AddContentFromHtml(htmlContent, section);
            AddContentWithHeaderFooter(doc, htmlContent);

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

        private void AddContentWithHeaderFooter(Document doc, string htmlContent)
        {
            // Configure global page setup
            PageSetup pageSetup = doc.DefaultPageSetup;
            pageSetup.PageWidth = Unit.FromCentimeter(21);    // A4 width
            pageSetup.PageHeight = Unit.FromCentimeter(29.7); // A4 height
            pageSetup.TopMargin = Unit.FromCentimeter(2.5);     // Reserve space for header
            pageSetup.BottomMargin = Unit.FromCentimeter(2.5);  // Reserve space for footer
            pageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            pageSetup.RightMargin = Unit.FromCentimeter(2.5);

            // Add header and footer for each section
            foreach (Section section in doc.Sections.Skip(1)) // Skip cover page
            {
                AddHeader(section, doc);
                AddFooter(section,doc);
            }

            // Process HTML content
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            foreach (HtmlNode node in htmlDoc.DocumentNode.ChildNodes)
            {
                Section section = doc.AddSection();
                ProcessHtmlNode(node, section);
            }
        }

        //private void AddHeader(Section section)
        //{
        //    HeaderFooter header = section.Headers.Primary;
        //    Paragraph headerParagraph = header.AddParagraph();
        //    headerParagraph.AddImage(HttpContext.Current.Server.MapPath("~/logo/header-logo.png"));
        //    headerParagraph.Format.Alignment = ParagraphAlignment.Center;
        //}

        //private void AddFooter(Section section)
        //{
        //    HeaderFooter footer = section.Footers.Primary;

        //    Table footerTable = footer.AddTable();
        //    footerTable.Borders.Width = 0;
        //    footerTable.AddColumn(Unit.FromCentimeter(16)); // Text column
        //    footerTable.AddColumn(Unit.FromCentimeter(5));  // Logo column

        //    Row footerRow = footerTable.AddRow();

        //    footerRow.Shading.Color = new Color(122, 181, 92);

        //    // Left-aligned footer text
        //    Paragraph leftText = footerRow.Cells[0].AddParagraph("© NOMADIX");
        //    leftText.Format.Font.Name = "Arial";
        //    leftText.Format.Font.Size = 10;
        //    leftText.Format.Font.Color = Colors.White;
        //    leftText.Format.Alignment = ParagraphAlignment.Left;

        //    // Right-aligned footer text
        //    Paragraph rightText = footerRow.Cells[1].AddParagraph("Last Updated\n10 January 2025");
        //    rightText.Format.Font.Name = "Arial";
        //    rightText.Format.Font.Size = 10;
        //    rightText.Format.Font.Color = Colors.White;
        //    rightText.Format.Alignment = ParagraphAlignment.Right;

        //    footerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;
        //    footerRow.Cells[1].VerticalAlignment = VerticalAlignment.Center;

        //    footerRow.Height = Unit.FromCentimeter(1.4);
        //}



        private void DefineStyles(Document doc)
        {
            Style style = doc.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 12;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        }

        private void AddHeader(Section section,Document doc)
        {
            HeaderFooter header = section.Headers.Primary;
            Paragraph headerParagraph = header.AddParagraph("Your Header Title Here");
            headerParagraph.Format.Alignment = ParagraphAlignment.Left;
            headerParagraph.Format.Font.Size = 14;
            headerParagraph.Format.Font.Bold = true;

            Paragraph line = header.AddParagraph();
            line.AddLineBreak();
            line.Format.Borders.Bottom.Width = 1.5;
        }

        //private void AddFooter(Section section)
        //{
        //    HeaderFooter footer = section.Footers.Primary;
        //    Paragraph footerParagraph = footer.AddParagraph("Page " + "{PAGE} of {NUMPAGES}");
        //    footerParagraph.Format.Alignment = ParagraphAlignment.Left;
        //    footerParagraph.Format.Font.Size = 10;
        //}
        private void AddFooter(Section section, Document doc)
        {
            PageSetup pageSetup = doc.DefaultPageSetup;

            // Define the height of the footer
            double footerHeight = Unit.FromCentimeter(1.5).Point; // Footer height
            pageSetup.BottomMargin = Unit.FromCentimeter(2); // Ensure enough space for footer + spacing buffer

            // Create a table for the footer content
            Table footerTable = section.Footers.Primary.AddTable();
            footerTable.Borders.Width = 0;
            footerTable.AddColumn(Unit.FromCentimeter(16)); // Text column
            footerTable.AddColumn(Unit.FromCentimeter(5));  // Logo column

            Row footerRow = footerTable.AddRow();

            // Set background color for the footer
            footerRow.Shading.Color = new Color(122, 181, 92); // Light green

            // Add left-aligned text
            Paragraph leftText = footerRow.Cells[0].AddParagraph();
            leftText.AddFormattedText("® NOMADIX", TextFormat.Bold);
            leftText.Format.Font.Name = "Arial";
            leftText.Format.Font.Size = 11;
            leftText.Format.Font.Color = Colors.White;
            leftText.Format.Alignment = ParagraphAlignment.Left;

            // Add right-aligned text (or logo)
            Paragraph rightText = footerRow.Cells[1].AddParagraph("Confidential");
            rightText.Format.Font.Name = "Arial";
            rightText.Format.Font.Size = 11;
            rightText.Format.Font.Color = Colors.White;
            rightText.Format.Alignment = ParagraphAlignment.Right;

            // Set vertical alignment for the footer cells
            footerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;
            footerRow.Cells[1].VerticalAlignment = VerticalAlignment.Center;

            // Adjust footer row height to match the specified footer height
            footerRow.Height = Unit.FromCentimeter(1.5); // Footer height
        }


        private void AddContentFromHtml(string htmlContent, Section section)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Select only the relevant elements (no divs or redundant nodes)
            var elements = htmlDoc.DocumentNode.SelectNodes("//p | //h1 | //h2 | //h3 | //ul | //ol | //table | //blockquote");

            if (elements != null)
            {
                foreach (HtmlNode node in elements)
                {
                    ProcessHtmlNode(node, section);
                }
            }
        }

        private void ProcessHtmlNode(HtmlNode node, Section section)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                string trimmedText = node.InnerText.Trim();
                if (!string.IsNullOrEmpty(trimmedText))
                {
                    Paragraph paragraph = section.AddParagraph(trimmedText);
                    paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                }
            }
            else if (node.Name == "h1" || node.Name == "h2" || node.Name == "h3")
            {
                Paragraph paragraph = section.AddParagraph(node.InnerText.Trim());
                paragraph.Format.Font.Size = node.Name == "h1" ? 16 : node.Name == "h2" ? 14 : 12;
                paragraph.Format.Font.Bold = true;
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
            }
            else if (node.Name == "p" && node.ParentNode?.Name != "li" && node.ParentNode?.Name != "td")
            {
                // Skip <p> tags inside <li> to avoid duplicate rendering
                Paragraph paragraph = section.AddParagraph(node.InnerText.Trim());
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
            }
            else if (node.Name == "ul")
            {
                foreach (HtmlNode li in node.SelectNodes("li"))
                {
                    Paragraph listItem = section.AddParagraph("• " + li.InnerText.Trim());
                    listItem.Format.LeftIndent = Unit.FromCentimeter(1);
                    listItem.Format.SpaceAfter = Unit.FromCentimeter(0.3);
                }
            }
            else if (node.Name == "ol")
            {
                int listIndex = 1;
                foreach (HtmlNode li in node.SelectNodes("li"))
                {
                    Paragraph listItem = section.AddParagraph($"{listIndex}. " + li.InnerText.Trim());
                    listItem.Format.LeftIndent = Unit.FromCentimeter(1);
                    listItem.Format.SpaceAfter = Unit.FromCentimeter(0.3);
                    listIndex++;
                }
            }
            else if (node.Name == "table")
            {
                AddTable(node, section);
            }
        }


        private void AddTable1(HtmlNode tableNode, Section section)
        {
            Table table = section.AddTable();
            table.Borders.Width = 0.5;

            // Define total available width for the table (A4 size minus margins, approx. 16 cm)
            double totalWidth = Unit.FromCentimeter(16).Centimeter;

            // Calculate column widths based on the number of columns
            HtmlNode firstRow = tableNode.SelectSingleNode(".//tr");
            int columnCount = firstRow?.SelectNodes("td | th")?.Count ?? 1;
            double columnWidth = totalWidth / columnCount;

            // Add columns to the table
            for (int i = 0; i < columnCount; i++)
            {
                Column column = table.AddColumn(Unit.FromCentimeter(columnWidth));
                column.Format.Alignment = ParagraphAlignment.Left; // Align content to the left
            }

            // Process rows
            HtmlNodeCollection rows = tableNode.SelectNodes(".//tr");
            if (rows != null)
            {
                foreach (HtmlNode row in rows)
                {
                    Row newRow = table.AddRow();
                    HtmlNodeCollection cells = row.SelectNodes("td | th");

                    for (int i = 0; i < (cells?.Count ?? 0); i++)
                    {
                        // Add content to the cell
                        string cellText = cells[i]?.InnerText?.Trim() ?? string.Empty;
                        newRow.Cells[i].AddParagraph(cellText);

                        // Apply formatting for alignment
                        newRow.Cells[i].Format.Alignment = ParagraphAlignment.Left; // Align text to the left
                    }
                }
            }
        }




        private string GetInnerText(HtmlNode node)
        {
            if (node == null) return string.Empty;

            return string.Join(" ", node.SelectNodes(".//text()")?.Select(textNode => textNode.InnerText.Trim()) ?? Array.Empty<string>());
        }


        private double ParseWidthFromStyle(string style)
        {
            if (string.IsNullOrEmpty(style)) return 0;

            string[] parts = style.Split(';');
            foreach (string part in parts)
            {
                if (part.Trim().StartsWith("width:", StringComparison.OrdinalIgnoreCase))
                {
                    string widthValue = part.Split(':')[1].Trim();
                    if (widthValue.EndsWith("px"))
                    {
                        if (double.TryParse(widthValue.Replace("px", ""), out double px))
                        {
                            return px / 37.795275591; // Convert px to cm
                        }
                    }
                    else if (widthValue.EndsWith("cm"))
                    {
                        if (double.TryParse(widthValue.Replace("cm", ""), out double cm))
                        {
                            return cm;
                        }
                    }
                }
            }
            return 0;
        }


        private bool IsNestedInTable(HtmlNode node)
        {
            // Check if this node is nested inside a table
            HtmlNode current = node.ParentNode;
            while (current != null)
            {
                if (current.Name == "table")
                {
                    return true;
                }
                current = current.ParentNode;
            }
            return false;
        }

        private void AddParagraph(HtmlNode node, Section section)
        {
            // Skip processing if the paragraph is inside a table
            if (!IsNestedInTable(node))
            {
                Paragraph paragraph = section.AddParagraph(node.InnerText.Trim());
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
            }
        }


        private void AddHeading(HtmlNode node, Section section)
        {
            Paragraph paragraph = section.AddParagraph(node.InnerText.Trim());
            paragraph.Format.Font.Size = node.Name == "h1" ? 16 : node.Name == "h2" ? 14 : 12;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
        }

        private void AddList(HtmlNode node, Section section)
        {
            foreach (HtmlNode li in node.SelectNodes("li"))
            {
                Paragraph listItem = section.AddParagraph("• " + li.InnerText.Trim());
                listItem.Format.LeftIndent = Unit.FromCentimeter(0.3);
                listItem.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            }
        }

        private void AddBlockquote(HtmlNode node, Section section)
        {
            Paragraph paragraph = section.AddParagraph(node.InnerText.Trim());
            paragraph.Format.Shading.Color = Colors.LightGray;
            paragraph.Format.Borders.Width = 0.5;
            paragraph.Format.LeftIndent = Unit.FromCentimeter(1);
            paragraph.Format.RightIndent = Unit.FromCentimeter(1);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
        }

        private void AddTable(HtmlNode tableNode, Section section)
        {
            ProcessTableNode(tableNode, section);
            //Table table = section.AddTable();
            //table.Borders.Width = 0.5;

            //// Define columns based on the first row
            //HtmlNode firstRow = tableNode.SelectSingleNode("tr");
            //if (firstRow != null)
            //{
            //    foreach (HtmlNode cell in firstRow.SelectNodes("td | th"))
            //    {
            //        table.AddColumn(Unit.FromCentimeter(5)); // Adjust width as needed
            //    }
            //}

            //// Populate the table
            //foreach (HtmlNode row in tableNode.SelectNodes("tr"))
            //{
            //    Row newRow = table.AddRow();
            //    int columnIndex = 0;
            //    foreach (HtmlNode cell in row.SelectNodes("td | th"))
            //    {
            //        newRow.Cells[columnIndex].AddParagraph(cell.InnerText.Trim());
            //        newRow.Cells[columnIndex].Format.Borders.Width = 0.5;
            //        columnIndex++;
            //    }
            //}
        }

        private bool IsNestedInBlock(HtmlNode node)
        {
            // Avoid processing the same text multiple times if it's inside blockquotes, divs, or tables
            return node.ParentNode != null &&
                   (node.ParentNode.Name == "blockquote" || node.ParentNode.Name == "table" || node.ParentNode.Name == "div");
        }

        private void ProcessTableNode(HtmlNode node, Section section)
        {
            Table table = new Table();
            table.Borders.Width = 0.75;

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
                                    tableCell.Format.SpaceBefore = Unit.FromPoint(5); // Simulate top padding
                                    tableCell.Format.SpaceAfter = Unit.FromPoint(5);  // Simulate bottom padding
                                    tableCell.Format.LeftIndent = Unit.FromPoint(5);  // Simulate left padding
                                    tableCell.Format.RightIndent = Unit.FromPoint(5); // Simulate right padding

                                    tableCell.Borders.Left.Width = 1;  // Adjust left border width
                                    tableCell.Borders.Top.Width = 1;  // Adjust top border width
                                    tableCell.Borders.Right.Width = 1; // Adjust right border width
                                    tableCell.Borders.Bottom.Width = 1; // Adjust bottom border width
                                }
                            }

                            // Handle rowspan (extend the cell vertically across rows)
                            if (rowspan > 1)
                            {
                                // For now, we won't fill in the rowspan, just adjust for layout
                                // You would need to implement row spanning manually, which MigraDoc does not natively support.
                            }
                        }
                    }
                }
            }

            FormatTable(table);
            section.Add(table);

            Paragraph spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(0.2); // Adjust spacing as needed
            spacer.Format.SpaceAfter = Unit.FromCentimeter(0.2);
            //Table table = section.AddTable();
            //table.Borders.Width = 0.5;

            //// Define columns based on the first row
            //HtmlNode firstRow = tableNode.SelectSingleNode("tr");
            //if (firstRow != null)
            //{
            //    foreach (HtmlNode cell in firstRow.SelectNodes("td | th"))
            //    {
            //        table.AddColumn(Unit.FromCentimeter(5)); // Adjust width as needed
            //    }
            //}

            //// Populate the table
            //foreach (HtmlNode row in tableNode.SelectNodes("tr"))
            //{
            //    Row newRow = table.AddRow();
            //    int columnIndex = 0;
            //    foreach (HtmlNode cell in row.SelectNodes("td | th"))
            //    {
            //        newRow.Cells[columnIndex].AddParagraph(cell.InnerText.Trim());
            //        newRow.Cells[columnIndex].Format.Borders.Width = 0.5;
            //        columnIndex++;
            //    }
            //}
        }

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
        }
    }
}


public class Article
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string HtmlContent { get; set; }
    public string CategoryId { get; set; }
}

public class ApiResponse<T>
{
    public T Data { get; set; }
}

public class Document360Service
{
    private readonly string apiKey = "YOUR_API_KEY";
    private readonly string baseUrl = "https://api.document360.io/v2";

    private HttpClient GetClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api_token", apiKey);
        return client;
    }

    public async Task<Article> GetArticle(string articleId)
    {
        var client = GetClient();
        var res = await client.GetAsync($"{baseUrl}/Articles/{articleId}");
        var json = await res.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ApiResponse<Article>>(json).Data;
    }

    public async Task<List<Article>> GetArticlesByCategory(string categoryId)
    {
        var client = GetClient();
        var res = await client.GetAsync($"{baseUrl}/Categories/{categoryId}/articles");
        var json = await res.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ApiResponse<List<Article>>>(json).Data;
    }
}

public static class PdfGenerator
{
    public static byte[] Generate(string html)
    {
        using (var ms = new MemoryStream())
        {
            using (var document = new iTextSharp.text.Document())
            {
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                using (var sr = new StringReader(html))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(
                        writer, document, sr
                    );
                }

                document.Close();
            }

            return ms.ToArray();
        }
    }
}

