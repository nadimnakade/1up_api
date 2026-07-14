using HtmlAgilityPack;
using Microsoft.Ajax.Utilities;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Newtonsoft.Json;
using PickupAPi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.UI.WebControls.WebParts;


namespace PickupAPi.Controllers
{
    

    //[EnableCors(origins: "https://nomadix-kms.document360.io", headers: "*", methods: "*")]
    [RoutePrefix("api/BusinessPdf")]
    public class _BusinessPdfController : ApiController
    {
        private static string API_TOKEN = "qstNVgrrO9A6w9byiy2c/n4Cza4lkaOLmXX9KXSx6yH4/0FDSBdOeSnP40bamD7jaJegf5sb0azs9GH1aOALH1qxM74IHURyIdvhC2ijW9tmHyc5TuLG5KOYibNfRaQ0mzMIffNJzcFqff5TtUFb8w==";


        int mainsrno = 1;
        int subsrno = 1;
        private readonly List<(string Title, int PageNumber)> blockquoteIndex = new List<(string, int)>();
        private List<(string Title, string Bookmark, string Level)> headings;
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
                // Ensure Montserrat font resolver is set before any PDF/font usage
                if (PdfSharp.Fonts.GlobalFontSettings.FontResolver == null)
                {
                    PdfSharp.Fonts.GlobalFontSettings.FontResolver = new PickupAPi.Utils.MontserratFontResolver();
                }

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


        private void PopulateIndexSection(Section indexSection, List<(string Title, string Bookmark, string Level)> headings)
        {
            // Validate input parameters
            if (indexSection == null || headings == null || !headings.Any())
            {
                return; // Exit if no headings to index
            }

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
            //AddHeader(indexSection);
            //AddFooter(indexSection);

            // Add title with enhanced styling
            Paragraph title = indexSection.AddParagraph("Table of Contents");
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.Font.Color = Color.FromRgb(81, 162, 198); // Match heading color
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.5);
            title.Format.SpaceBefore = Unit.FromCentimeter(0.5);

            // Add a decorative line under the title
            Paragraph line = indexSection.AddParagraph();
            line.Format.Borders.Bottom.Width = 1;
            line.Format.Borders.Bottom.Color = Color.FromRgb(81, 162, 198);
            line.Format.SpaceAfter = Unit.FromCentimeter(1);

            // Create a table for the index entries
            Table indexTable = indexSection.AddTable();
            indexTable.Borders.Visible = false;
            indexTable.AddColumn(Unit.FromCentimeter(1.5));  // Number column
            indexTable.AddColumn(Unit.FromCentimeter(12));   // Title column
            indexTable.AddColumn(Unit.FromCentimeter(2.5));  // Page number column

            // Add table header
            Row headerRow = indexTable.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Font.Size = 12;
            headerRow.Shading.Color = Color.FromRgb(240, 240, 240);
            headerRow.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            headerRow.Format.SpaceBefore = Unit.FromCentimeter(0.5);


            // Ensure a consistent minimum header height
            headerRow.Height = Unit.FromCentimeter(0.3);
            headerRow.HeightRule = RowHeightRule.AtLeast;

            // Apply padding / spacing to each header cell for better visual spacing
            for (int ci = 0; ci < headerRow.Cells.Count; ci++)
            {
                var cell = headerRow.Cells[ci];
                // Horizontal "padding"
                cell.Format.LeftIndent = Unit.FromCentimeter(0.3);
                cell.Format.RightIndent = Unit.FromCentimeter(0.3);

                // Vertical spacing inside the cell
                cell.Format.SpaceBefore = Unit.FromPoint(0.5);
                cell.Format.SpaceAfter = Unit.FromPoint(0.3);

                // Center cell content vertically
                cell.VerticalAlignment = VerticalAlignment.Center;

                // Ensure header text alignment defaults are preserved (Page column centered later)
                cell.Format.Alignment = ParagraphAlignment.Left;
            }

            headerRow.Cells[0].AddParagraph("#");
            headerRow.Cells[1].AddParagraph("Section");
            headerRow.Cells[2].AddParagraph("Page");
            headerRow.Cells[2].Format.Alignment = ParagraphAlignment.Left;

            // Add headings to the index with hierarchical structure
            int h2Number = 1;
            int h3Number = 1;


            // Add a blank spacer row at the top of the index entries
            Row blankRow = indexTable.AddRow();
            blankRow.Height = Unit.FromCentimeter(0.4);
            blankRow.HeightRule = RowHeightRule.AtLeast;
            // Clear any default content and hide borders for the blank row
            for (int ci = 0; ci < blankRow.Cells.Count; ci++)
            {
                blankRow.Cells[ci].AddParagraph(string.Empty);
                blankRow.Cells[ci].Borders.Visible = false;
            }

            foreach (var heading in headings)
            {
                Row row = indexTable.AddRow();
                row.Format.Font.Size = 10;

                if (heading.Level == "H2")
                {
                    // Reset H3 counter for new H2 section
                    h3Number = 1;

                    // Section number for H2
                    Paragraph numberPara = row.Cells[0].AddParagraph(h2Number.ToString());
                    numberPara.Format.Font.Bold = true;
                    numberPara.Format.Font.Color = Color.FromRgb(81, 162, 198);

                    // Title with hyperlink for H2
                    Paragraph titlePara = row.Cells[1].AddParagraph();
                    Hyperlink hyperlink = titlePara.AddHyperlink(heading.Bookmark, HyperlinkType.Bookmark);
                    hyperlink.AddText(heading.Title);
                    hyperlink.Font.Color = Color.FromRgb(51, 51, 51);
                    hyperlink.Font.Underline = Underline.Single;
                    hyperlink.Font.Bold = true; // H2 titles are bold

                    h2Number++;
                }
                else if (heading.Level == "H3")
                {
                    // Subsection number for H3
                    Paragraph numberPara = row.Cells[0].AddParagraph($"{h2Number - 1}.{h3Number}");
                    numberPara.Format.Font.Color = Color.FromRgb(120, 120, 120);

                    // Title with hyperlink for H3 (indented)
                    Paragraph titlePara = row.Cells[1].AddParagraph();
                    titlePara.Format.LeftIndent = Unit.FromCentimeter(0.8); // Indent H3 titles
                    Hyperlink hyperlink = titlePara.AddHyperlink(heading.Bookmark, HyperlinkType.Bookmark);
                    hyperlink.AddText(heading.Title);
                    hyperlink.Font.Color = Color.FromRgb(80, 80, 80);
                    hyperlink.Font.Underline = Underline.Single;

                    h3Number++;
                }
                // Page number reference - make the page number clickable to the same bookmark
                Paragraph pagePara = row.Cells[2].AddParagraph();

                // Create a bookmark hyperlink and add a PageRef field inside it so the displayed page
                // number is clickable and navigates to the heading bookmark.
                var pageLink = pagePara.AddHyperlink(heading.Bookmark, HyperlinkType.Bookmark);
                pageLink.AddPageRefField(heading.Bookmark);
                pageLink.Font.Color = Color.FromRgb(81, 162, 198);
                pageLink.Font.Underline = Underline.Single;

                pagePara.Format.Alignment = ParagraphAlignment.Center;

                // Add subtle row spacing
                row.Format.SpaceBefore = Unit.FromPoint(0.5);
                row.Format.SpaceAfter = Unit.FromPoint(6);
            }

            //// Add footer note
            //Paragraph footerNote = indexSection.AddParagraph();
            //footerNote.Format.SpaceBefore = Unit.FromCentimeter(2);
            //footerNote.AddText("Click on any section title to navigate directly to that page.");
            //footerNote.Format.Font.Size = 8;
            //footerNote.Format.Font.Italic = true;
            //footerNote.Format.Font.Color = Colors.Gray;
            //footerNote.Format.Alignment = ParagraphAlignment.Center;
        }

        private Section CreateAndInsertTocSection(Document doc)
        {
            // Create a new section for the TOC
            Section tocSection = new Section();

            // Set up page formatting
            tocSection.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21),    // A4 width
                PageHeight = Unit.FromCentimeter(29.7),  // A4 height
                TopMargin = Unit.FromCentimeter(2.5),
                BottomMargin = Unit.FromCentimeter(2.5),
                LeftMargin = Unit.FromCentimeter(2.5),
                RightMargin = Unit.FromCentimeter(2.5)
            };

            // Insert the TOC section after the cover page (position 1)
            doc.Sections.InsertObject(1, tocSection);

            return tocSection;
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
            AddCoverPage(doc);

            // Initialize headings list for index
            headings = new List<(string Title, string Bookmark, string Level)>();

            // Create and insert the TOC section before processing content
            Section tocSection = CreateAndInsertTocSection(doc);

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
            pageSetup.HeaderDistance = Unit.FromCentimeter(0.8);
            pageSetup.FooterDistance = Unit.FromCentimeter(0.8);

            // Process HTML content (this will populate headings list)
            ProcessHtmlContent(htmlContent, contentSection);

            // Prepare document so that PageRef fields and bookmark hyperlinks resolve correctly
            var migraRenderer = new MigraDoc.Rendering.DocumentRenderer(doc);
            migraRenderer.PrepareDocument();

            // Now that page numbers are known, populate the index section
            PopulateIndexSection(tocSection, headings);

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
            Paragraph logoText = headerRow.Cells[1].AddParagraph("Nomadix");
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

        //private void AddFooter(Section section)
        //{
        //    HeaderFooter footer = section.Footers.Primary;

        //    // Add 12pt margin between body and footer
        //    Paragraph spacer = footer.AddParagraph();
        //    spacer.Format.SpaceBefore = Unit.FromPoint(14);

        //    // Add a horizontal line above the footer
        //    Paragraph line = footer.AddParagraph();
        //    line.Format.Borders.Top.Width = 0.05;
        //    line.Format.Borders.Top.Color = Colors.LightGray;

        //    // Create a table for the footer
        //    Table footerTable = footer.AddTable();
        //    footerTable.Borders.Width = 0;
        //    footerTable.AddColumn(Unit.FromCentimeter(15)); // Main content column
        //    footerTable.AddColumn(Unit.FromCentimeter(1)); // Page number column

        //    Row footerRow = footerTable.AddRow();
        //    footerRow.Height = Unit.FromCentimeter(0.8);

        //    // Add year and confidentiality text in one line
        //    string currentMonthYear = DateTime.Now.ToString("MMMM yyyy");
        //    Paragraph mainParagraph = footerRow.Cells[0].AddParagraph();
        //    mainParagraph.Format.TabStops.AddTabStop("3cm");
        //    mainParagraph.AddText(currentMonthYear);
        //    mainParagraph.AddTab();
        //    mainParagraph.AddText("Information subject to change without notice");
        //    mainParagraph.Format.Font.Size = 8;
        //    mainParagraph.Format.Font.Color = Colors.Gray; // Lighter gray color
        //    mainParagraph.Format.Font.Bold = false; // Remove bold for lighter appearance
        //    mainParagraph.Format.Alignment = ParagraphAlignment.Left;
        //    footerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;

        //    // Add page number to the right
        //    Paragraph pageNumberParagraph = footerRow.Cells[1].AddParagraph();
        //    pageNumberParagraph.AddPageField();
        //    pageNumberParagraph.Format.Font.Size = 8;
        //    pageNumberParagraph.Format.Font.Color = Colors.Gray; // Lighter gray color
        //    pageNumberParagraph.Format.Font.Bold = false; // Remove bold for lighter appearance
        //    pageNumberParagraph.Format.Alignment = ParagraphAlignment.Right;
        //    footerRow.Cells[1].VerticalAlignment = VerticalAlignment.Center;
        //}
        private void AddFooter(Section section)
        {
            HeaderFooter footer = section.Footers.Primary;

            // Add 12pt margin between body and footer
            Paragraph spacer = footer.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromPoint(14);

            // Add a horizontal line above the footer
            Paragraph line = footer.AddParagraph();
            line.Format.Borders.Top.Width = 0.05;
            line.Format.Borders.Top.Color = Colors.LightGray;

            // Create a table for the footer
            Table footerTable = footer.AddTable();
            footerTable.Borders.Width = 0;

            // *** CHANGED: Use 3 columns for 3 distinct alignments ***
            // (Adjust widths as needed)
            footerTable.AddColumn(Unit.FromCentimeter(4));  // Column 1: Date (Left-aligned)
            footerTable.AddColumn(Unit.FromCentimeter(10)); // Column 2: Notice (Center-aligned)
            footerTable.AddColumn(Unit.FromCentimeter(2));  // Column 3: Page # (Right-aligned)

            Row footerRow = footerTable.AddRow();
            footerRow.Height = Unit.FromCentimeter(0.8);

            // *** NEW: Cell 0 - Add date (Left-aligned) ***
            string currentMonthYear = DateTime.Now.ToString("MMMM yyyy");
            Paragraph dateParagraph = footerRow.Cells[0].AddParagraph();
            dateParagraph.AddText(currentMonthYear);
            dateParagraph.Format.Font.Size = 8;
            dateParagraph.Format.Font.Color = Colors.Gray;
            dateParagraph.Format.Font.Bold = false;
            dateParagraph.Format.Alignment = ParagraphAlignment.Left;
            footerRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;

            // *** NEW: Cell 1 - Add notice (Center-aligned) ***
            Paragraph noticeParagraph = footerRow.Cells[1].AddParagraph();
            noticeParagraph.AddText("Information subject to change without notice");
            noticeParagraph.Format.Font.Size = 8;
            noticeParagraph.Format.Font.Color = Colors.Gray;
            noticeParagraph.Format.Font.Bold = false;
            noticeParagraph.Format.LeftIndent = Unit.FromCentimeter(0.01);
            noticeParagraph.Format.Alignment = ParagraphAlignment.Center; // <-- Goal achieved
            footerRow.Cells[1].VerticalAlignment = VerticalAlignment.Center;

            // *** UPDATED: Cell 2 - Add page number (Right-aligned) ***
            // (Note: This is now Cells[2], not Cells[1])
            Paragraph pageNumberParagraph = footerRow.Cells[2].AddParagraph();
            pageNumberParagraph.AddPageField();
            pageNumberParagraph.Format.Font.Size = 8;
            pageNumberParagraph.Format.Font.Color = Colors.Gray;
            pageNumberParagraph.Format.Font.Bold = false;
            pageNumberParagraph.Format.Alignment = ParagraphAlignment.Right;
            footerRow.Cells[2].VerticalAlignment = VerticalAlignment.Center;
        }

        [HttpGet]
        [Route("test-article")]
        public async Task<IHttpActionResult> TestArticle()
        {
            string articleUrl = "https://kms.cloud.global/trustedwifi/docs/administration";

            var result = await GetArticleByUrl(articleUrl);

            return Ok(result);
        }
        
        private const string API_BASE = "https://apihub.document360.io";
        private const string LANG_CODE = "en";

        // ─── 1. Resolve article by public URL ───────────────────────────────────────
        private async Task<string> GetArticleByUrl(string articleUrl)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api_token", API_TOKEN);

            string url = $"{API_BASE}/v2/Articles?url={HttpUtility.UrlEncode(articleUrl)}&isPublished=true";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // ─── 2. Get all articles belonging to a category ────────────────────────────
        private async Task<string> GetCategoryArticles(string categoryId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api_token", API_TOKEN);

            // Returns the category tree including child_categories and articles[]
            string url = $"{API_BASE}/v2/Categories/{categoryId}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // ─── 3. Get full article content (with language) ────────────────────────────
        private async Task<string> GetArticleDetail(string articleId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api_token", API_TOKEN);

            // BUG FIX: include lang code — without it the endpoint may 404 or return no content
            string url = $"{API_BASE}/v2/Articles/{articleId}/{LANG_CODE}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // ─── 4. Flatten nested category tree into a flat article ID list ─────────────
        private List<string> ExtractArticleIds(dynamic categoryData)
        {
            var ids = new List<string>();

            // Articles directly in this category
            if (categoryData.articles != null)
                foreach (var a in categoryData.articles)
                    ids.Add((string)a.id);

            // Recurse into child categories
            if (categoryData.child_categories != null)
                foreach (var child in categoryData.child_categories)
                    ids.AddRange(ExtractArticleIds(child));

            return ids;
        }

        // ─── 5. Main endpoint ────────────────────────────────────────────────────────
        [HttpPost]
        [Route("GenerateFromUrl_Old")]
        public async Task<HttpResponseMessage> GenerateFromUrl_Old([FromBody] UrlRequestModel req)
        {
            if (string.IsNullOrWhiteSpace(req?.Url))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "URL missing");

            try
            {
                // Step 1 — resolve article from URL
                var articleRes = await GetArticleByUrl(req.Url);
                dynamic articleObj = JsonConvert.DeserializeObject(articleRes);

                if (articleObj?.data == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Article not found for the given URL");

                // ✅ BUG FIX: use category_id, NOT id
                string categoryId = (string)articleObj.data.category_id;

                if (string.IsNullOrEmpty(categoryId))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Article has no category_id");

                // Step 2 — fetch full category tree
                var categoryRes = await GetCategoryArticles(categoryId);
                dynamic categoryObj = JsonConvert.DeserializeObject(categoryRes);

                if (categoryObj?.data == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Category not found");

                // Step 3 — flatten all article IDs from the category tree
                List<string> articleIds = ExtractArticleIds(categoryObj.data);

                if (!articleIds.Any())
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No articles found in category");

                // Step 4 — fetch each article's content and accumulate HTML
                var fullHtml = new StringBuilder();
                foreach (string articleId in articleIds)
                {
                    try
                    {
                        var detailRes = await GetArticleDetail(articleId);
                        dynamic detailObj = JsonConvert.DeserializeObject(detailRes);

                        string title = (string)detailObj?.data?.title ?? "";
                        string content = (string)detailObj?.data?.html_content ?? "";

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            // Wrap each article with a heading so the PDF has clear separation
                            fullHtml.Append($"<h1>{System.Net.WebUtility.HtmlEncode(title)}</h1>");
                            fullHtml.Append(content);
                            fullHtml.Append("<hr style='page-break-after:always;'/>");
                        }
                    }
                    catch (Exception articleEx)
                    {
                        // Skip articles that fail individually — don't abort the whole export
                        Console.WriteLine($"Skipping article {articleId}: {articleEx.Message}");
                    }
                }

                if (fullHtml.Length == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No content could be retrieved");

                // Step 5 — generate PDF
                byte[] pdfBytes = GenerateBusinessPdf(fullHtml.ToString(), 0);

                string categoryName = (string)categoryObj.data.name ?? "Section";
                string safeFileName = string.Concat(categoryName.Split(Path.GetInvalidFileNameChars()));

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(pdfBytes);
                response.Content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition =
                    new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = $"{safeFileName}.pdf"
                    };
                return response;
            }
            catch (HttpRequestException httpEx)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway,
                    $"Document360 API unreachable: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ─── Cache folder (set once, reuse everywhere) ───────────────────────────────
        private static readonly string PDF_CACHE_DIR =
            HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/App_Data/PdfCache")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfCache");

        // ─── Helper: build a deterministic, safe file path from a URL ───────────────
        private string GetCacheFilePath(string articleUrl)
        {
            // SHA256 the URL so any URL becomes a safe, unique, fixed-length filename
            var sha = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(articleUrl.Trim().ToLowerInvariant()));
            string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return Path.Combine(PDF_CACHE_DIR, $"{hashHex}.pdf");
        }

        // ─── Helper: read cached PDF (returns null if not cached) ───────────────────
        private byte[] TryGetCachedPdf(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"[PDF Cache] HIT → {filePath}");
                    return File.ReadAllBytes(filePath);
                }
            }
            catch (Exception ex)
            {
                // Cache read failure is non-fatal — just regenerate
                Console.WriteLine($"[PDF Cache] Read error: {ex.Message}");
            }
            return null;
        }

        // ─── Helper: persist PDF to disk ────────────────────────────────────────────
        private void SavePdfToCache(string filePath, byte[] pdfBytes)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllBytes(filePath, pdfBytes);
                Console.WriteLine($"[PDF Cache] SAVED → {filePath}");
            }
            catch (Exception ex)
            {
                // Cache write failure is non-fatal — response still goes out fine
                Console.WriteLine($"[PDF Cache] Write error: {ex.Message}");
            }
        }

        // ─── Helper: build the HttpResponseMessage from raw bytes ───────────────────
        private HttpResponseMessage BuildPdfResponse(byte[] pdfBytes, string categoryName)
        {
            string safeFileName = string.Concat(
                (categoryName ?? "Section").Split(Path.GetInvalidFileNameChars()));

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(pdfBytes);
            response.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"{safeFileName}.pdf"
                };
            return response;
        }

        // ─── Main endpoint ───────────────────────────────────────────────────────────
        [HttpPost]
        [Route("GenerateFromUrl")]
        public async Task<HttpResponseMessage> GenerateFromUrl([FromBody] UrlRequestModel req)
        {
            if (string.IsNullOrWhiteSpace(req?.Url))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "URL missing");

            // ✅ Check cache BEFORE hitting Document360 API
            string cacheFilePath = GetCacheFilePath(req.Url);
            //byte[] cachedPdf = TryGetCachedPdf(cacheFilePath);
            byte[] cachedPdf = (req.ForceRefresh == true) ? null : TryGetCachedPdf(cacheFilePath);


            // Wipe stale cache file when force-refreshing
            if (req.ForceRefresh == true && File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
                Console.WriteLine($"[PDF Cache] CLEARED (force refresh) → {cacheFilePath}");
            }


            if (cachedPdf != null)
            {
                // Derive a display name from the URL path segment as we don't have
                // the API category name at this point (it was never fetched)
                string cachedName = Path.GetFileNameWithoutExtension(
                    new Uri(req.Url).Segments.LastOrDefault() ?? "Document");
                return BuildPdfResponse(cachedPdf, cachedName);
            }

            try
            {
                // Step 1 — resolve article from URL
                var articleRes = await GetArticleByUrl(req.Url);
                dynamic articleObj = JsonConvert.DeserializeObject(articleRes);

                if (articleObj?.data == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Article not found");

                string categoryId = (string)articleObj.data.category_id;
                if (string.IsNullOrEmpty(categoryId))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Article has no category_id");

                // Step 2 — fetch category tree
                var categoryRes = await GetCategoryArticles(categoryId);
                dynamic categoryObj = JsonConvert.DeserializeObject(categoryRes);

                if (categoryObj?.data == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Category not found");

                string categoryName = (string)categoryObj.data.name ?? "Section";

                // Step 3 — flatten article IDs
                List<string> articleIds = ExtractArticleIds(categoryObj.data);
                if (!articleIds.Any())
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No articles found");

                // Step 4 — accumulate HTML
                var fullHtml = new StringBuilder();
                foreach (string articleId in articleIds)
                {
                    try
                    {
                        var detailRes = await GetArticleDetail(articleId);
                        dynamic detailObj = JsonConvert.DeserializeObject(detailRes);

                        string title = (string)detailObj?.data?.title ?? "";
                        string content = (string)detailObj?.data?.html_content ?? "";

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            fullHtml.Append($"<h1>{System.Net.WebUtility.HtmlEncode(title)}</h1>");
                            fullHtml.Append(content);
                            fullHtml.Append("<hr style='page-break-after:always;'/>");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Article] Skipping {articleId}: {ex.Message}");
                    }
                }

                if (fullHtml.Length == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No content retrieved");

                // Step 5 — generate PDF
                byte[] pdfBytes = GenerateBusinessPdf(fullHtml.ToString(), 0);

                // ✅ Save to cache so next request skips all the above
                SavePdfToCache(cacheFilePath, pdfBytes);

                return BuildPdfResponse(pdfBytes, categoryName);
            }
            catch (HttpRequestException httpEx)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway,
                    $"Document360 API unreachable: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ─── Optional: bust the cache for a specific URL ─────────────────────────────
        [HttpDelete]
        [Route("ClearCache")]
        public HttpResponseMessage ClearCache([FromBody] UrlRequestModel req)
        {
            if (string.IsNullOrWhiteSpace(req?.Url))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "URL missing");

            string filePath = GetCacheFilePath(req.Url);

            if (!File.Exists(filePath))
                return Request.CreateResponse(HttpStatusCode.NotFound, "No cached PDF for this URL");

            File.Delete(filePath);
            return Request.CreateResponse(HttpStatusCode.OK, "Cache cleared");
        }

        // ─── Optional: force-regenerate regardless of cache ──────────────────────────
        // Just pass forceRefresh: true in the request body
        public class UrlRequestModel
        {
            public string Url { get; set; }
            public bool? ForceRefresh { get; set; } = false;  // ← add this
        }


        [HttpPost]
        [Route("GenerateFullSectionPdf")]
        public async Task<HttpResponseMessage> GenerateFullSectionPdf([FromBody] ArticleRequest req)
        {
            if (string.IsNullOrEmpty(req?.ArticleId))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid ArticleId");

            using (var client = new HttpClient())
            {
                try
                {
                    // 🔹 STEP 1: Get article detail
                    var articleRes = await client.GetStringAsync(
                        $"https://kms.cloud.global/api/document/get-article-detail?articleId={req.ArticleId}&lang=en&version-slug=trustedwifi");

                    dynamic articleObj = JsonConvert.DeserializeObject(articleRes);
                    string categoryId = articleObj.data.categoryId;

                    // 🔹 STEP 2: Get all articles in category
                    var categoryRes = await client.GetStringAsync(
                        $"https://kms.cloud.global/api/document/get-category-articles?categoryId={categoryId}&lang=en&version-slug=trustedwifi");

                    dynamic categoryObj = JsonConvert.DeserializeObject(categoryRes);

                    var articles = categoryObj.data.articles;

                    // 🔹 STEP 3: Sort properly
                    var sortedArticles = ((IEnumerable<dynamic>)articles)
                        .OrderBy(a => (int)a.order)
                        .ToList();

                    List<string> allHtml = new List<string>();

                    // 🔹 STEP 4: Fetch each article HTML
                    foreach (var art in sortedArticles)
                    {
                        var res = await client.GetStringAsync(
                            $"https://kms.cloud.global/api/document/get-article-detail?articleId={art.id}&lang=en&version-slug=trustedwifi");

                        dynamic obj = JsonConvert.DeserializeObject(res);

                        string title = obj.data.title;
                        string html = obj.data.content;

                        if (!string.IsNullOrEmpty(html))
                        {
                            html = $"<h1>{title}</h1>" + html;
                            allHtml.Add(html);
                        }
                    }

                    // 🔹 STEP 5: Merge HTML
                    StringBuilder finalHtml = new StringBuilder();

                    foreach (var html in allHtml)
                    {
                        finalHtml.Append("<div style='page-break-before:always'></div>");
                        finalHtml.Append(html);
                    }

                    // 🔹 STEP 6: Generate PDF (your existing method)
                    byte[] pdfBytes = GenerateBusinessPdf(finalHtml.ToString(), 0);

                    // 🔹 STEP 7: Return response
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(pdfBytes)
                    };

                    response.Content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                    response.Content.Headers.ContentDisposition =
                        new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                        {
                            FileName = "FullSection.pdf"
                        };

                    return response;
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        private List<int> headingNumbers = new List<int>();

        private void DefineStyles(Document doc)
        {
            // Normal text style
            Style normal = doc.Styles["Normal"];
            normal.Font.Name = "Montserrat";
            normal.Font.Size = 10;

            // Heading styles
            Style heading1 = doc.Styles["Heading1"];
            heading1.Font.Name = "Arial";
            heading1.Font.Size = 24;
            heading1.Font.Bold = true;
            heading1.Font.Color = new Color(28, 74, 113); // Nomadix blue
            heading1.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(2);
            heading1.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(2);

            Style heading2 = doc.Styles["Heading2"];
            heading2.Font.Name = "Arial";
            heading2.Font.Size = 18;
            heading2.Font.Bold = true;
            heading2.Font.Color = Colors.Black;
            heading2.ParagraphFormat.SpaceBefore = Unit.FromPoint(9); // IS-003: 8pt before
            heading2.ParagraphFormat.SpaceAfter = Unit.FromPoint(7);  // IS-003: 6pt after

            Style heading3 = doc.Styles["Heading3"];
            heading3.Font.Name = "Arial";
            heading3.Font.Size = 16;
            heading3.Font.Bold = false;
            heading3.Font.Color = Colors.Black;
            heading3.ParagraphFormat.SpaceBefore = Unit.FromPoint(14); // 12pt padding before subheadings
            heading3.ParagraphFormat.SpaceAfter = Unit.FromPoint(8);   // 6pt after for clear separation

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
                            AddHeading(section, childNode.InnerText, "Heading1", 24);
                            break;
                        case "h2":
                            AddHeading(section, childNode.InnerText, "Heading2", 18);
                            subsrno++;
                            break;
                        case "h3":
                            AddHeading(section, childNode.InnerText, "Heading3", 16);
                            subsrno = 1;
                            mainsrno++;
                            break;
                        case "h4":
                            AddHeading(section, childNode.InnerText, "Heading4", 14);
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
            //Paragraph spacer = section.AddParagraph();
            //spacer.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            //spacer.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            // Create a table to structure the section with rounded appearance
            Table table = section.AddTable();
            table.Borders.Width = 0;
            //table.Rows.LeftIndent = Unit.FromCentimeter(0.2);
            table.Format.SpaceBefore = Unit.FromCentimeter(0.05);
            table.Format.SpaceAfter = Unit.FromCentimeter(0.05);

            // Add columns: one for the icon, one for the text content
            //table.AddColumn(Unit.FromCentimeter(1.8)); // Icon column (slightly wider)
            table.AddColumn(Unit.FromCentimeter(16)); // Text column

            // Add a row to the table
            Row row = table.AddRow();
            row.HeightRule = RowHeightRule.AtLeast;
            row.Height = Unit.FromCentimeter(1.2); // Minimum height that adjusts to content

            // Set the background color and padding
            row.Shading.Color = backgroundColor;
            row.Format.SpaceBefore = Unit.FromCentimeter(0.06);
            row.Format.SpaceAfter = Unit.FromCentimeter(0.06);

            //// Add the icon cell
            //Cell iconCell = row.Cells[0];
            //iconCell.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            //iconCell.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            //iconCell.VerticalAlignment = VerticalAlignment.Center;
            //iconCell.Format.Alignment = ParagraphAlignment.Center;

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

            //// Apply left border to the first cell only
            //iconCell.Borders.Left.Width = Unit.FromPoint(4);
            //iconCell.Borders.Left.Color = borderColor;

            // Add icon based on title type
            string defaultIconPath = GetDefaultIconPath(title);
            string finalIconPath = !string.IsNullOrEmpty(iconPath) ? iconPath : defaultIconPath;

            //if (!string.IsNullOrEmpty(finalIconPath) && File.Exists(finalIconPath))
            //{
            //    try
            //    {
            //        Paragraph iconPara = iconCell.AddParagraph();
            //        iconPara.Format.Alignment = ParagraphAlignment.Center;
            //        var iconImage = iconPara.AddImage(finalIconPath);
            //        iconImage.LockAspectRatio = true;
            //        iconImage.Width = Unit.FromCentimeter(1.0);
            //        iconImage.Height = Unit.FromCentimeter(1.0);
            //        iconImage.Left = ShapePosition.Center;
            //        iconImage.Top = ShapePosition.Center;
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Error adding icon image: {ex.Message}");
            //        // Add text icon as fallback
            //        AddTextIcon(iconCell, title, Colors.White);
            //    }
            //}
            //else
            //{
            //    // Add text icon as fallback
            //    AddTextIcon(iconCell, title, Colors.White);
            //}

            // Add the content cell
            Cell contentCell = row.Cells[0];
            contentCell.Format.SpaceBefore = Unit.FromCentimeter(0.35);
            contentCell.Format.SpaceAfter = Unit.FromCentimeter(0.25);
            contentCell.VerticalAlignment = VerticalAlignment.Top;
            contentCell.Format.LeftIndent = Unit.FromCentimeter(0.4);

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
            titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

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
                contentParagraph.Format.LineSpacing = Unit.FromCentimeter(0.25);
                contentParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.25);
            }

            // Add spacing after section
            Paragraph spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromCentimeter(0.01);
            spacer.Format.SpaceAfter = Unit.FromCentimeter(0.01);
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

        //private void ProcessListContent(Cell contentCell, string mainContent, Color textColor)
        //{
        //    var htmlDoc = new HtmlAgilityPack.HtmlDocument();
        //    htmlDoc.LoadHtml(mainContent);

        //    // Normal text before lists
        //    var textNodes = htmlDoc.DocumentNode.ChildNodes
        //        .Where(n => n.NodeType == HtmlNodeType.Text)
        //        .Select(n => n.InnerText.Trim())
        //        .Where(t => !string.IsNullOrWhiteSpace(t));

        //    foreach (var text in textNodes)
        //    {
        //        Paragraph textPara = contentCell.AddParagraph();
        //        textPara.AddText(text);
        //        textPara.Format.Font.Color = textColor;
        //        textPara.Format.Font.Size = 10;
        //        textPara.Format.SpaceAfter = Unit.FromCentimeter(0.1);
        //    }

        //    // Lists
        //    var lists = htmlDoc.DocumentNode.SelectNodes("//ul");
        //    if (lists != null)
        //    {
        //        foreach (var list in lists)
        //        {
        //            var items = list.SelectNodes("./li");
        //            if (items != null)
        //            {
        //                foreach (var item in items)
        //                {
        //                    Paragraph listItemPara = contentCell.AddParagraph();
        //                    listItemPara.Format.LeftIndent = Unit.FromCentimeter(0.3);

        //                    // Check for paragraphs with images inside list items
        //                    var paragraphNodes = item.SelectNodes(".//p");
        //                    if (paragraphNodes != null && paragraphNodes.Count > 0)
        //                    {
        //                        // Add bullet point first
        //                        listItemPara.AddText("• ");

        //                        foreach (var pNode in paragraphNodes)
        //                        {
        //                            // Process text content first
        //                            var _textNodes = pNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text);
        //                            foreach (var textNode in _textNodes)
        //                            {
        //                                if (!string.IsNullOrWhiteSpace(textNode.InnerText))
        //                                {
        //                                    listItemPara.AddText(WebUtility.HtmlDecode(textNode.InnerText.Trim()));
        //                                }
        //                            }

        //                            // Process line breaks
        //                            var brNodes = pNode.SelectNodes(".//br");
        //                            if (brNodes != null)
        //                            {
        //                                foreach (var br in brNodes)
        //                                {
        //                                    listItemPara.AddLineBreak();
        //                                }
        //                            }

        //                            // Process images
        //                            var imgNodes = pNode.SelectNodes(".//img");
        //                            if (imgNodes != null)
        //                            {
        //                                foreach (var img in imgNodes)
        //                                {
        //                                    string imageUrl = img.GetAttributeValue("src", string.Empty);
        //                                    imageUrl = HttpUtility.HtmlDecode(imageUrl);

        //                                    if (!string.IsNullOrEmpty(imageUrl))
        //                                    {
        //                                        AddImageToParaSection(imageUrl, contentCell.Section);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // Regular list item without paragraphs
        //                        listItemPara.AddText("• " + WebUtility.HtmlDecode(item.InnerText.Trim()));
        //                    }

        //                    listItemPara.Format.Font.Color = textColor;
        //                    listItemPara.Format.Font.Size = 10;
        //                    listItemPara.Format.SpaceBefore = Unit.FromCentimeter(0.05);
        //                    listItemPara.Format.SpaceAfter = Unit.FromCentimeter(0.05);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void ProcessListContent(Cell contentCell, string mainContent, Color textColor)
        //{
        //    var htmlDoc = new HtmlAgilityPack.HtmlDocument();
        //    htmlDoc.LoadHtml(mainContent);

        //    // Normal text before lists
        //    var textNodes = htmlDoc.DocumentNode.ChildNodes
        //        .Where(n => n.NodeType == HtmlNodeType.Text)
        //        .Select(n => n.InnerText.Trim())
        //        .Where(t => !string.IsNullOrWhiteSpace(t));

        //    foreach (var text in textNodes)
        //    {
        //        Paragraph textPara = contentCell.AddParagraph();
        //        textPara.AddText(text);
        //        textPara.Format.Font.Color = textColor;
        //        textPara.Format.Font.Size = 10;
        //        textPara.Format.SpaceAfter = Unit.FromCentimeter(0.1);
        //    }

        //    // Lists
        //    var lists = htmlDoc.DocumentNode.SelectNodes("//ul");
        //    if (lists != null)
        //    {
        //        foreach (var list in lists)
        //        {
        //            var items = list.SelectNodes("./li");
        //            if (items != null)
        //            {
        //                foreach (var item in items)
        //                {
        //                    Paragraph listItemPara = contentCell.AddParagraph();
        //                    listItemPara.Format.LeftIndent = Unit.FromCentimeter(0.3);

        //                    // Add bullet point
        //                    listItemPara.AddText("• ");

        //                    // Process <p> content (text + images)
        //                    var paragraphNodes = item.SelectNodes(".//p");
        //                    if (paragraphNodes != null)
        //                    {
        //                        foreach (var pNode in paragraphNodes)
        //                        {
        //                            // Text
        //                            foreach (var textNode in pNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text))
        //                            {
        //                                if (!string.IsNullOrWhiteSpace(textNode.InnerText))
        //                                    listItemPara.AddText(WebUtility.HtmlDecode(textNode.InnerText.Trim()));
        //                            }

        //                            // Images
        //                            var imgNodes = pNode.SelectNodes(".//img");
        //                            if (imgNodes != null)
        //                            {
        //                                foreach (var img in imgNodes)
        //                                {
        //                                    string imageUrl = HttpUtility.HtmlDecode(img.GetAttributeValue("src", ""));
        //                                    if (!string.IsNullOrEmpty(imageUrl))
        //                                        AddImageToParaSection(imageUrl, contentCell.Section);
        //                                }
        //                            }

        //                            // Line breaks
        //                            if (pNode.SelectNodes(".//br") != null)
        //                                listItemPara.AddLineBreak();
        //                        }
        //                    }

        //                    // Process <table> inside <li> — must be added to Section
        //                    var tableNodes = item.SelectNodes(".//table");
        //                    if (tableNodes != null)
        //                    {
        //                        foreach (var tableNode in tableNodes)
        //                        {
        //                            RenderHtmlTable(contentCell.Section, tableNode, textColor); // Pass Section, not Cell
        //                        }
        //                    }

        //                    listItemPara.Format.Font.Color = textColor;
        //                    listItemPara.Format.Font.Size = 10;
        //                    listItemPara.Format.SpaceBefore = Unit.FromCentimeter(0.05);
        //                    listItemPara.Format.SpaceAfter = Unit.FromCentimeter(0.05);
        //                }
        //            }
        //        }
        //    }
        //}

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
                    if (items == null) continue;

                    foreach (var item in items)
                    {
                        Paragraph listItemPara = contentCell.AddParagraph();
                        //listItemPara.Format.LeftIndent = Unit.FromCentimeter(0.02);
                        //listItemPara.Format.FirstLineIndent = Unit.FromCentimeter(-0.02);
                        listItemPara.AddText("• ");

                        // Process child nodes of li
                        foreach (var child in item.ChildNodes)
                        {
                            if (child.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                            {
                                // Text and images inside <p>
                                foreach (var pChild in child.ChildNodes)
                                {
                                    if (pChild.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string imageUrl = HttpUtility.HtmlDecode(pChild.GetAttributeValue("src", ""));
                                        if (!string.IsNullOrEmpty(imageUrl))
                                            AddImageToParaSection(imageUrl, contentCell.Section);
                                    }
                                    else
                                    {
                                        listItemPara.AddText(WebUtility.HtmlDecode(pChild.InnerText));
                                    }
                                }
                            }
                            else if (child.Name.Equals("div", StringComparison.OrdinalIgnoreCase))
                            {
                                // Look for tables inside divs
                                var tableNodes = child.SelectNodes(".//table");
                                if (tableNodes != null)
                                {
                                    foreach (var tableNode in tableNodes)
                                    {
                                        AddTableToDocument(listItemPara, tableNode);
                                    }
                                }
                            }
                            else if (child.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                            {
                                string imageUrl = HttpUtility.HtmlDecode(child.GetAttributeValue("src", ""));
                                if (!string.IsNullOrEmpty(imageUrl))
                                    AddImageToParaSection(imageUrl, contentCell.Section);
                            }
                            else
                            {
                                // Plain text
                                listItemPara.AddText(WebUtility.HtmlDecode(child.InnerText));
                            }
                        }

                        listItemPara.Format.Font.Color = textColor;
                        listItemPara.Format.Font.Size = 10;
                        listItemPara.Format.SpaceBefore = Unit.FromCentimeter(0.05);
                        listItemPara.Format.SpaceAfter = Unit.FromCentimeter(0.05);
                    }
                }
            }
        }


        private void RenderHtmlTable(Section section, HtmlNode tableNode, Color textColor)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            var firstRow = tableNode.SelectSingleNode(".//tr");
            int colCount = firstRow?.SelectNodes("./th|./td")?.Count ?? 1;

            for (int i = 0; i < colCount; i++)
                table.AddColumn(Unit.FromCentimeter(16.0 / colCount));

            foreach (var rowNode in tableNode.SelectNodes(".//tr"))
            {
                var row = table.AddRow();
                int colIndex = 0;

                foreach (var cellNode in rowNode.SelectNodes("./th|./td"))
                {
                    var cell = row.Cells[colIndex];
                    var para = cell.AddParagraph();
                    para.Format.Font.Color = textColor;

                    // Text
                    string text = WebUtility.HtmlDecode(cellNode.InnerText.Trim());
                    if (!string.IsNullOrEmpty(text))
                        para.AddText(text);

                    // Images
                    var imgNode = cellNode.SelectSingleNode(".//img");
                    if (imgNode != null)
                    {
                        string src = HttpUtility.HtmlDecode(imgNode.GetAttributeValue("src", ""));
                        if (!string.IsNullOrEmpty(src))
                            AddImageToParaSection(src, section); // Add image to section
                    }

                    colIndex++;
                }
            }
        }


        private void AddHeading(Section section, string text, string style, int fontSize)
        {
            string strContext = "";
            if (string.IsNullOrWhiteSpace(text)) return;

            Paragraph heading = section.AddParagraph();
            heading.Style = style;
            heading.Format.Font.Size = isFirst ? 24 : fontSize;
            // Bold rule: H1/H2 bold; H3/H4 not bold
            if (style == "Heading1" || style == "Heading2")
                heading.Format.Font.Bold = true;
            else
                heading.Format.Font.Bold = false;
            // Color rule: H1 uses brand color, H2/H3 black
            if (style == "Heading1")
                heading.Format.Font.Color = new Color(28, 74, 113);
            else
                heading.Format.Font.Color = Colors.Black;
            // Avoid headings orphaned at the bottom of the page
            heading.Format.KeepWithNext = true;
            heading.Format.KeepTogether = true;

            // Clean up the text (remove HTML tags and decode entities)
            string cleanText = WebUtility.HtmlDecode(StripHtml(text)).Trim();

            // Create bookmark for H2 and H3 headings for index navigation
            if (style == "Heading2" || style == "Heading3")
            {
                string level = style == "Heading2" ? "H2" : "H3";
                string bookmarkName = $"{level.ToLower()}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                heading.AddBookmark(bookmarkName);

                // Store heading info for index generation
                if (headings == null)
                    headings = new List<(string Title, string Bookmark, string Level)>();
                headings.Add((cleanText, bookmarkName, level));
            }

            //heading.AddText(strContext + " " + cleanText);
            heading.AddText(cleanText);

            isFirst = false;
        }

        private void AddParagraph(Section section, HtmlNode node)
        {
            if (node == null) return;

            Paragraph para = section.AddParagraph();
            // prevent paragraph splitting across pages and try to keep with next logical block
            para.Format.KeepTogether = true;
            para.Format.KeepWithNext = false; // set true if next element should remain on same page

            // Process the paragraph content (may contain spans, links, etc.)
            ProcessInlineElements(para, node);
        }

        //private void AddImageToParaSection(string imageUrl, Section section)
        //{
        //    try
        //    {
        //        // Decode the URL to ensure any special characters are handled properly
        //        string decodedUrl = HttpUtility.HtmlDecode(imageUrl);

        //        // Extract the file name and extension from the URL
        //        string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
        //        string tempImagePath = Path.Combine(Path.GetTempPath(), fileName);

        //        using (WebClient client = new WebClient())
        //        {
        //            // Set headers to mimic a browser request (optional, but recommended)
        //            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        //            // Download the image to the temporary path with the correct extension
        //            client.DownloadFile(decodedUrl, tempImagePath);
        //        }

        //        // Add the image to the section
        //        Paragraph paragraph = section.AddParagraph();
        //        //paragraph.Format.LeftIndent = Unit.FromCentimeter(2);
        //        //paragraph.Format.RightIndent = Unit.FromCentimeter(2);
        //        var image = paragraph.AddImage(tempImagePath);

        //        // Calculate the full width of the page
        //        //Unit pageWidth = section.PageSetup.PageWidth;
        //        //Unit leftMargin = section.PageSetup.LeftMargin;
        //        //Unit rightMargin = section.PageSetup.RightMargin;
        //        //Unit fullWidth = pageWidth - leftMargin - rightMargin;

        //        // Set image dimensions (adjust as needed)
        //        image.Width = 430; // Adjust width
        //        image.LockAspectRatio = true; // Maintain aspect ratio
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error processing image: {ex.StackTrace}");
        //    }
        //}

        //private void AddImageToParaSection(string imageUrl, Section section)
        // {
        //    try
        //    {
        //        string decodedUrl = HttpUtility.HtmlDecode(imageUrl);
        //        decodedUrl = CleanUrl(decodedUrl);
        //        string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
        //        string tempImagePath = Path.Combine(Path.GetTempPath(), fileName);

        //        using (WebClient client = new WebClient())
        //        {
        //            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        //            client.DownloadFile(decodedUrl, tempImagePath);
        //        }

        //        // Add the image to the section
        //        Paragraph paragraph = section.AddParagraph();
        //        var image = paragraph.AddImage(tempImagePath);
        //        image.LockAspectRatio = true;

        //        // Calculate usable page width (account for margins)
        //        Unit pageWidth = section.PageSetup.PageWidth;
        //        Unit leftMargin = section.PageSetup.LeftMargin;
        //        Unit rightMargin = section.PageSetup.RightMargin;
        //        Unit usableWidth = pageWidth - leftMargin - rightMargin;

        //        // Set a default target width and clamp to the usable width
        //        Unit targetWidth = Unit.FromCentimeter(15);
        //        if (targetWidth > usableWidth)
        //        {
        //            targetWidth = usableWidth;
        //        }
        //        image.Width = targetWidth;

        //        // Center the image in the page
        //        paragraph.Format.Alignment = ParagraphAlignment.Center;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error processing image: {ex.Message}");
        //    }
        //}



        //private void AddImagetoPara(Paragraph para, HtmlNode node)
        //{
        //    string src = node.GetAttributeValue("src", "");
        //    if (string.IsNullOrEmpty(src))
        //        return;

        //    try
        //    {
        //        para.Format.Alignment = ParagraphAlignment.Center;

        //        if (src.StartsWith("http") || src.StartsWith("https"))
        //        {
        //            // Download external image
        //            using (WebClient client = new WebClient())
        //            {
        //                byte[] imageData = client.DownloadData(src);
        //                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
        //                File.WriteAllBytes(tempPath, imageData);

        //                var image = para.AddImage(tempPath);
        //                image.LockAspectRatio = true;

        //                // Compute usable page width (paragraph-level image)
        //                Section section = para.Section;
        //                Unit containerWidth = section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin;

        //                // Default target width of 15cm, but never exceed container width
        //                Unit targetWidth = Unit.FromCentimeter(15);
        //                if (targetWidth > containerWidth)
        //                {
        //                    targetWidth = containerWidth;
        //                }
        //                image.Width = targetWidth;

        //                // Clean up temp file
        //                File.Delete(tempPath);
        //            }
        //        }
        //        else if (src.StartsWith("~/"))
        //        {
        //            // Local image
        //            string localPath = HttpContext.Current.Server.MapPath(src);
        //            if (File.Exists(localPath))
        //            {
        //                var image = para.AddImage(localPath);
        //                image.LockAspectRatio = true;

        //                // Compute usable page width (paragraph-level image)
        //                Section section = para.Section;
        //                Unit containerWidth = section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin;

        //                // Default target width of 15cm, but never exceed container width
        //                Unit targetWidth = Unit.FromCentimeter(15);
        //                if (targetWidth > containerWidth)
        //                {
        //                    targetWidth = containerWidth;
        //                }
        //                image.Width = targetWidth;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log error and add placeholder text
        //        Console.WriteLine($"Error adding image {src}: {ex.Message}");

        //    }
        //}

        private void AddImageToParaSection(string imageUrl, Section section)
        {
            string tempImagePath = null;
            try
            {
                string decodedUrl = HttpUtility.HtmlDecode(imageUrl);
                decodedUrl = CleanUrl(decodedUrl); // Assuming you have this
                string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
                // Using Guid to prevent name collisions
                tempImagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_" + fileName);

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    client.DownloadFile(decodedUrl, tempImagePath);
                }

                // --- Get native width ---
                Unit imageNativeWidth;
                using (var img = System.Drawing.Image.FromFile(tempImagePath))
                {
                    double widthInPoints = (double)img.Width / img.HorizontalResolution * 72;
                    imageNativeWidth = Unit.FromPoint(widthInPoints);
                }

                // --- Add image with correct size ---
                Paragraph paragraph = section.AddParagraph();
                var image = paragraph.AddImage(tempImagePath);
                image.LockAspectRatio = true;
                paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                Unit pageWidth = section.PageSetup.PageWidth;
                Unit leftMargin = section.PageSetup.LeftMargin;
                Unit rightMargin = section.PageSetup.RightMargin;
                Unit usableWidth = pageWidth - leftMargin - rightMargin;

                double finalWidthInPoints = Math.Min(imageNativeWidth.Point, usableWidth.Point);
                image.Width = Unit.FromPoint(finalWidthInPoints);

                paragraph.Format.Alignment = ParagraphAlignment.Center;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
            }
            // NO 'finally' BLOCK - The temp file is intentionally left behind
            // This is a resource leak, but it matches your original code's behavior.
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
                            case "p":
                                // Process span content recursively
                                ProcessInlineElements(para, childNode);
                                break;
                            case "figure":

                                // Images
                                var imgNode = childNode.SelectSingleNode(".//img");
                                if (imgNode != null)
                                {
                                    string src = HttpUtility.HtmlDecode(imgNode.GetAttributeValue("src", ""));
                                    if (!string.IsNullOrEmpty(src))
                                        AddImageToParaSection(src, para.Section); // Add image to section
                                }

                                //// Handle figures with nested images and optional captions
                                //var nestedImg = childNode.SelectSingleNode(".//img");
                                //if (nestedImg != null)
                                //{
                                //    var _imageUrl = nestedImg.GetAttributeValue("src", string.Empty);
                                //    _imageUrl = HttpUtility.HtmlDecode(_imageUrl);
                                //    _imageUrl = CleanUrl(_imageUrl);
                                //    if (!string.IsNullOrEmpty(_imageUrl))
                                //    {
                                //        AddImageToParaSection(_imageUrl, para.Section);
                                //    }
                                //}
                                //// Optional figcaption rendering
                                //var captionNode = childNode.SelectSingleNode(".//figcaption");
                                //if (captionNode != null && !string.IsNullOrWhiteSpace(captionNode.InnerText))
                                //{
                                //    var captionText = WebUtility.HtmlDecode(captionNode.InnerText.Trim());
                                //    var ft = para.AddFormattedText(captionText);
                                //    ft.Italic = true;
                                //    para.AddLineBreak();
                                //}
                                break;
                            case "br":
                                para.AddLineBreak();
                                break;
                            case "img":
                                string imageUrl = childNode.GetAttributeValue("src", string.Empty);
                                imageUrl = HttpUtility.HtmlDecode(imageUrl);
                                imageUrl = CleanUrl(imageUrl);

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    AddImageToParaSection(imageUrl, para.Section);
                                }
                                break;
                            case "video":
                                // PDFs do not support embedded video; render a clickable URL fallback
                                var videoUrl = childNode.GetAttributeValue("src", string.Empty);
                                videoUrl = HttpUtility.HtmlDecode(videoUrl);
                                videoUrl = CleanUrl(videoUrl);


                                const string THUMBNAIL_PATH = "https://www.citypng.com/public/uploads/preview/video-cinema-clap-black-icon-png-image-7017516950353797kbrvcrxz0.png";
                                if (!string.IsNullOrEmpty(videoUrl) && File.Exists(THUMBNAIL_PATH))
                                {
                                    // 1. Create the Hyperlink object within the paragraph
                                    //var hyperlink = para.AddHyperlink(videoUrl, MigraDoc.DocumentObjectModel.HyperlinkType.Web);
                                    var hyperlink = para.AddHyperlink(videoUrl, HyperlinkType.Web);
                                    // 2. Add the Image as the content of the Hyperlink object
                                    MigraDoc.DocumentObjectModel.Shapes.Image image = hyperlink.AddImage(THUMBNAIL_PATH);
                                    para.Format.SpaceBefore = Unit.FromCentimeter(0.5);
                                    para.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                                    // 3. Set image dimensions and styling (on the image object)
                                    image.Width = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(5);
                                    image.Height = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(3);
                                    image.LockAspectRatio = true;

                                    // NOTE: Layout properties like WrapFormat and Left/Right/Top/Bottom 
                                    // need to be set on the Image object itself.

                                    var _linkText = hyperlink.AddFormattedText("Click here to view Video");
                                    _linkText.Color = new Color(0, 106, 138);
                                    _linkText.Underline = Underline.Single;
                                    para.AddLineBreak();
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

            var firstrowindex = 0;
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
                            // Inside a table cell: use the current cell's column width minus padding
                            int colIndex = cell.Column.Index;
                            Unit colWidth = cell.Table.Columns[colIndex].Width;
                            AddImageToParagraph(paragraph, content, colWidth - Unit.FromCentimeter(0.2));
                        }
                        else if (content.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                        {
                            // Render inline content inside <p> within table cells, preserving <strong>/<b>
                            foreach (var pContent in content.ChildNodes)
                            {
                                if (pContent.InnerText == "Site Code")
                                {
                                    var a = "";
                                }
                                if (pContent.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                                {


                                    // Direct image handling for images in paragraphs inside table cells
                                    var src = pContent.GetAttributeValue("src", "");
                                    if (!string.IsNullOrEmpty(src))
                                    {
                                        try
                                        {
                                            int colIndex2 = cell.Column.Index;
                                            Unit colWidth2 = cell.Table.Columns[colIndex2].Width;
                                            AddImageToParagraph(paragraph, pContent, colWidth2 - Unit.FromCentimeter(0.2));
                                        }
                                        catch (Exception ex)
                                        {
                                            paragraph.AddText("[Image could not be loaded - ]" + ex.Message);
                                            Debug.WriteLine($"Image load error: {ex.Message}");
                                        }
                                    }
                                }
                                else if (pContent.Name.Equals("strong", StringComparison.OrdinalIgnoreCase) ||
                                         pContent.Name.Equals("b", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(pContent.InnerText));
                                    ft.Bold = true;
                                }
                                else if (pContent.Name.Equals("em", StringComparison.OrdinalIgnoreCase) ||
                                         pContent.Name.Equals("i", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(pContent.InnerText));
                                    ft.Italic = true;
                                }
                                else if (pContent.Name.Equals("u", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(pContent.InnerText));
                                    ft.Underline = Underline.Single;
                                }
                                else if (pContent.NodeType == HtmlNodeType.Text)
                                {
                                    var text = pContent.InnerText;
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        if (firstrowindex == 0)
                                        {
                                            var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(text));
                                            ft.Bold = true;
                                        }
                                        else
                                        {
                                            paragraph.AddText(WebUtility.HtmlDecode(text));
                                        }
                                    }
                                }
                                else
                                {
                                    // Fallback: add decoded inner text
                                    var text = pContent.InnerText;
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        if (firstrowindex == 0)
                                        {
                                            var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(text));
                                            ft.Bold = true;

                                        }
                                        else
                                        {
                                            paragraph.AddText(WebUtility.HtmlDecode(text));
                                        }
                                        
                                    }


                                }
                            }
                            
                            paragraph.Format.SpaceAfter = Unit.FromPoint(6);
                            paragraph.Format.SpaceBefore = Unit.FromPoint(6);
                        }
                        else
                        {
                            if (firstrowindex == 0)
                            {
                                var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(content.InnerText));
                                ft.Bold = true;

                            }
                            else
                            {
                                paragraph.AddText(WebUtility.HtmlDecode(content.InnerText));
                            }                            
                            paragraph.Format.SpaceAfter = Unit.FromPoint(6);
                            paragraph.Format.SpaceBefore = Unit.FromPoint(6);

                            
                        }
                    }
                }
                if(firstrowindex == 0)
                    newRow.Shading.Color = new Color(180, 180, 180);
                firstrowindex++;
            }
        }

        //private void AddImageToParagraph(Paragraph para, HtmlNode imgNode)
        //{

        //    var src = imgNode.GetAttributeValue("src", "");
        //    if (!string.IsNullOrEmpty(src))
        //    {
        //        try
        //        {
        //            using (var client = new WebClient())
        //            {
        //                client.Headers.Add("User-Agent", "Mozilla/5.0");
        //                var tempPath = Path.GetTempFileName();

        //                try
        //                {
        //                    client.DownloadFile(src, tempPath);

        //                    var image = para.AddImage(tempPath);

        //                    // Read attributes
        //                    var widthAttr = imgNode.GetAttributeValue("width", "");
        //                    var styleAttr = imgNode.GetAttributeValue("style", "");

        //                    // Extract width from style="width:..."
        //                    int styleWidth = 0;
        //                    if (!string.IsNullOrEmpty(styleAttr))
        //                    {
        //                        var widthMatch = Regex.Match(styleAttr, @"width\s*:\s*([0-9]+)px", RegexOptions.IgnoreCase);
        //                        if (widthMatch.Success)
        //                        {
        //                            int.TryParse(widthMatch.Groups[1].Value, out styleWidth);
        //                        }
        //                    }

        //                    // Available max width (page or column – here I assume ~16 cm for A4 minus margins)
        //                    Unit maxWidth = Unit.FromCentimeter(16);

        //                    bool sizeSet = false;

        //                    // Priority 1: HTML width attribute
        //                    if (!string.IsNullOrEmpty(widthAttr) && int.TryParse(widthAttr, out int widthPx))
        //                    {
        //                        image.Width = Unit.FromPoint(widthPx * 0.75); // px → pt
        //                        sizeSet = true;
        //                    }
        //                    // Priority 2: CSS style width
        //                    else if (styleWidth > 0)
        //                    {
        //                        image.Width = Unit.FromPoint(styleWidth * 0.75); // px → pt
        //                        sizeSet = true;
        //                    }
        //                    // Priority 3: Default fit
        //                    if (!sizeSet)
        //                    {
        //                        image.LockAspectRatio = true;
        //                        image.Width = maxWidth;
        //                    }

        //                    // Final safeguard: clamp to max width
        //                    if (image.Width > maxWidth)
        //                    {
        //                        image.LockAspectRatio = true;
        //                        image.Width = maxWidth;
        //                    }

        //                    // Center image
        //                    para.Format.Alignment = ParagraphAlignment.Center;
        //                    image.LockAspectRatio = true;
        //                }
        //                finally
        //                {
        //                    if (File.Exists(tempPath))
        //                    {
        //                        File.Delete(tempPath);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            para.AddText("[Image could not be loaded - AddImageToParagraph]" + ex.Message);
        //            Debug.WriteLine($"Image load error: {ex.Message}");
        //        }
        //    }
        //}
        private void AddImageToParagraph(Paragraph para, HtmlNode imgNode, Unit? containerWidthOverride = null)
        {
            var src = imgNode.GetAttributeValue("src", "");
            if (!string.IsNullOrEmpty(src))
            {
                try
                {
                    // Use the same approach as the working method
                    string decodedUrl = HttpUtility.HtmlDecode(src);
                    decodedUrl = CleanUrl(decodedUrl);
                    string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
                    string tempPath = Path.Combine(Path.GetTempPath(), fileName);

                    using (var client = new WebClient())
                    {
                        client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                        client.DownloadFile(decodedUrl, tempPath);
                    }

                    // Add image
                    var image = para.AddImage(tempPath);
                    image.LockAspectRatio = true;
                    para.Format.SpaceBefore = Unit.FromCentimeter(0.5);
                    para.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                    // Determine container width: override or usable page width
                    Unit containerWidth;
                    if (containerWidthOverride.HasValue)
                    {
                        containerWidth = containerWidthOverride.Value;
                    }
                    else
                    {
                        Section section = para.Section;
                        Unit pageWidth = section.PageSetup.PageWidth;
                        Unit leftMargin = section.PageSetup.LeftMargin;
                        Unit rightMargin = section.PageSetup.RightMargin;
                        containerWidth = pageWidth - leftMargin - rightMargin;
                    }

                    // Explicitly set a target width and clamp to container width
                    Unit targetWidth = Unit.FromCentimeter(15);
                    if (targetWidth > containerWidth)
                    {
                        targetWidth = containerWidth;
                    }
                    image.Width = targetWidth;

                    // Center the image
                    para.Format.Alignment = ParagraphAlignment.Center;
                }
                catch (Exception ex)
                {
                    para.AddText($"[Image could not be loaded] {ex.Message}");
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
                Paragraph itemPara = para.Section.AddParagraph();
                itemPara.Format.LeftIndent = Unit.FromCentimeter(1.0);
                itemPara.Format.FirstLineIndent = Unit.FromCentimeter(-0.2);
                itemPara.Format.SpaceBefore = Unit.FromPoint(6);
                itemPara.Format.SpaceAfter = Unit.FromPoint(6);
                itemPara.Format.LineSpacing = Unit.FromPoint(10);

                if (isOrdered)
                {
                    itemPara.AddText($"{itemNumber}. ");
                    itemNumber++;
                }
                else
                {
                    itemPara.AddText("• ");
                }

                var paragraphNodes = listItem.SelectNodes(".//p");
                if (paragraphNodes != null && paragraphNodes.Count > 0)
                {
                    foreach (var pNode in paragraphNodes)
                    {
                        var textNodes = pNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text);
                        foreach (var textNode in textNodes)
                        {
                            if (!string.IsNullOrWhiteSpace(textNode.InnerText))
                                itemPara.AddText(WebUtility.HtmlDecode(textNode.InnerText.Trim()));
                        }

                        var brNodes = pNode.SelectNodes(".//br");
                        if (brNodes != null)
                        {
                            foreach (var br in brNodes)
                                itemPara.AddLineBreak();
                        }

                        var imgNodes = pNode.SelectNodes(".//img");
                        if (imgNodes != null)
                        {
                            foreach (var img in imgNodes)
                            {
                                string imageUrl = img.GetAttributeValue("src", string.Empty);
                                imageUrl = HttpUtility.HtmlDecode(imageUrl);
                                if (!string.IsNullOrEmpty(imageUrl))
                                    AddImageToParaSection(imageUrl, itemPara.Section);
                            }
                        }
                    }
                }
                else
                {
                    ProcessInlineElements(itemPara, listItem);
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
                para.Format.LeftIndent = Unit.FromCentimeter(1.0);
                para.Format.FirstLineIndent = Unit.FromCentimeter(-0.2);
                para.Format.SpaceBefore = Unit.FromPoint(6);
                para.Format.SpaceAfter = Unit.FromPoint(6);
                para.Format.LineSpacing = Unit.FromPoint(10);

                if (isOrdered)
                {
                    para.AddText($"{itemNumber}. ");
                    itemNumber++;
                }
                else
                {
                    para.AddText("• ");
                }

                ProcessInlineElements(para, listItem);
            }
        }

        private static bool HasBoldStyle(HtmlNode node)
        {
            var style = node.GetAttributeValue("style", "")?.ToLowerInvariant();
            if (string.IsNullOrEmpty(style)) return false;
            // crude but effective check
            return style.Contains("font-weight:") && (style.Contains("bold") || style.Contains("700") || style.Contains("800") || style.Contains("900"));
        }

        private void AddTable(Section section, HtmlNode node)
        {
            if (node == null) return;

            // Start table on a fresh page (as you had)
            var pageBreak = section.AddParagraph();
            pageBreak.Format.PageBreakBefore = true;
            pageBreak.Format.SpaceBefore = Unit.FromCentimeter(0.5);

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            // Determine number of columns from the first row
            var firstRow = node.SelectSingleNode(".//tr");
            if (firstRow == null) return;

            // Note: use child axis ("./") here so we don't double-count nested cells
            var firstRowCells = firstRow.SelectNodes("./th|./td");
            if (firstRowCells == null || firstRowCells.Count == 0) return;
            int columnCount = firstRowCells.Count;

            // Dynamic widths
            double[] columnWidths = CalculateColumnWidths(node, columnCount);
            for (int i = 0; i < columnCount; i++)
                table.AddColumn(Unit.FromCentimeter(columnWidths[i]));

            // Rows
            var rows = node.SelectNodes(".//tr");
            if (rows == null) return;

            bool isHeaderRow = true;
            foreach (var rowNode in rows)
            {
                if (IsBlankRow(rowNode)) continue;

                var row = table.AddRow();
                var cellNodes = rowNode.SelectNodes("./th|./td");
                if (cellNodes == null) continue;

                for (int i = 0; i < cellNodes.Count && i < table.Columns.Count; i++)
                {
                    var cellNode = cellNodes[i];
                    var cell = row.Cells[i];

                    // Header styling (matches your previous logic)
                    if (isHeaderRow || cellNode.Name.Equals("th", StringComparison.OrdinalIgnoreCase))
                    {
                        cell.Shading.Color = new Color(191, 191, 191);
                        cell.Format.Font.Bold = true;
                        cell.Format.SpaceBefore = Unit.FromPoint(6);
                        cell.Format.SpaceAfter = Unit.FromPoint(6);
                        cell.Format.LeftIndent = Unit.FromPoint(4);
                        cell.Format.RightIndent = Unit.FromPoint(4);
                    }
                    else
                    {
                        cell.Format.SpaceBefore = Unit.FromPoint(4);
                        cell.Format.SpaceAfter = Unit.FromPoint(4);
                        cell.Format.LeftIndent = Unit.FromPoint(4);
                        cell.Format.RightIndent = Unit.FromPoint(4);
                    }

                    // === Replicated content handling from AddTableToDocument ===
                    var paragraph = cell.AddParagraph();
                    if (isHeaderRow)
                        paragraph.Format.Font.Bold = true;     // belt & suspenders

                    foreach (var content in cellNode.ChildNodes)
                    {
                        if (content.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                        {
                            // Fit image to current column width minus a small padding
                            int colIndex = cell.Column.Index;
                            Unit colWidth = cell.Table.Columns[colIndex].Width;
                            AddImageToParagraph(paragraph, content, colWidth - Unit.FromCentimeter(0.2));
                        }
                        else if (content.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var pContent in content.ChildNodes)
                            {

                                if (pContent.InnerText == "The hotel or site name.")
                                {
                                    var a = "";
                                }

                                if (pContent.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        int colIndex2 = cell.Column.Index;
                                        Unit colWidth2 = cell.Table.Columns[colIndex2].Width;
                                        AddImageToParagraph(paragraph, pContent, colWidth2 - Unit.FromCentimeter(0.2));
                                    }
                                    catch (Exception ex)
                                    {
                                        paragraph.AddText("[Image could not be loaded - ]" + ex.Message);
                                        Debug.WriteLine($"Image load error: {ex.Message}");
                                    }
                                }
                                else if (pContent.Name.Equals("strong", StringComparison.OrdinalIgnoreCase) ||
                                         pContent.Name.Equals("b", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(pContent.InnerText));
                                    ft.Bold = true;
                                }
                                else if (pContent.Name.Equals("em", StringComparison.OrdinalIgnoreCase) ||
                                         pContent.Name.Equals("i", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(pContent.InnerText));
                                    ft.Italic = true;
                                }
                                else if (pContent.Name.Equals("u", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(pContent.InnerText));
                                    ft.Underline = Underline.Single;
                                }
                                else if (pContent.NodeType == HtmlNodeType.Text)
                                {
                                    var text = pContent.InnerText;
                                    if (!string.IsNullOrWhiteSpace(text))
                                        paragraph.AddText(WebUtility.HtmlDecode(text));
                                }
                                else
                                {
                                    var text = pContent.InnerText;
                                    if (!string.IsNullOrWhiteSpace(text))
                                        paragraph.AddText(WebUtility.HtmlDecode(text));
                                }
                            }

                            paragraph.Format.SpaceBefore = Unit.FromPoint(6);
                            paragraph.Format.SpaceAfter = Unit.FromPoint(6);
                        }
                        else if (content.NodeType == HtmlNodeType.Text)
                        {
                            var text = content.InnerText;
                            if (!string.IsNullOrWhiteSpace(text))
                                paragraph.AddText(WebUtility.HtmlDecode(text));

                            paragraph.Format.SpaceBefore = Unit.FromPoint(6);
                            paragraph.Format.SpaceAfter = Unit.FromPoint(6);
                        }
                        else
                        {
                            var text = content.InnerText;
                            if (!string.IsNullOrWhiteSpace(text))
                                paragraph.AddText(WebUtility.HtmlDecode(text));

                            paragraph.Format.SpaceBefore = Unit.FromPoint(6);
                            paragraph.Format.SpaceAfter = Unit.FromPoint(6);
                        }
                    }
                    // === end replicated content handling ===
                }

                isHeaderRow = false;
            }
        }


        private void AddTable1(Section section, HtmlNode node)
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
                    string imageUrl = content.GetAttributeValue("src", "");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        try
                        {
                            string decodedUrl = HttpUtility.HtmlDecode(imageUrl);
                            decodedUrl = CleanUrl(decodedUrl);
                            string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
                            string tempImagePath = Path.Combine(Path.GetTempPath(), fileName);

                            using (WebClient client = new WebClient())
                            {
                                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                                client.DownloadFile(decodedUrl, tempImagePath);
                            }

                            Paragraph imagePara = cell.AddParagraph();
                            MigraDoc.DocumentObjectModel.Shapes.Image image = imagePara.AddImage(tempImagePath);
                            imagePara.Format.SpaceBefore = Unit.FromCentimeter(0.5);
                            imagePara.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                            // Available width inside this cell
                            int colIndex = cell.Column.Index;
                            Unit colWidth = cell.Table.Columns[colIndex].Width;
                            Unit maxImageWidth = colWidth - Unit.FromCentimeter(0.2); // padding

                            // Read HTML attributes
                            string width = content.GetAttributeValue("width", "");
                            string height = content.GetAttributeValue("height", "");

                            bool sizeSet = false;

                            // Width handling
                            if (!string.IsNullOrEmpty(width))
                            {
                                if (width.Equals("auto", StringComparison.OrdinalIgnoreCase))
                                {
                                    image.LockAspectRatio = true;
                                    image.Width = maxImageWidth;
                                    sizeSet = true;
                                }
                                else if (int.TryParse(width.Replace("px", ""), out int pxWidth))
                                {
                                    image.Width = Unit.FromPoint(pxWidth * 0.75); // px → pt
                                    sizeSet = true;
                                }
                            }

                            // Height handling
                            if (!string.IsNullOrEmpty(height))
                            {
                                if (height.Equals("auto", StringComparison.OrdinalIgnoreCase))
                                {
                                    image.LockAspectRatio = true;
                                }
                                else if (int.TryParse(height.Replace("px", ""), out int pxHeight))
                                {
                                    image.Height = Unit.FromPoint(pxHeight * 0.75); // px → pt
                                }
                            }

                            // If no size specified, fit to column width
                            if (!sizeSet)
                            {
                                image.LockAspectRatio = true;
                                image.Width = maxImageWidth;
                            }

                            // Ensure image never exceeds cell width
                            if (image.Width > maxImageWidth)
                            {
                                image.LockAspectRatio = true;
                                image.Width = maxImageWidth;
                            }

                            imagePara.Format.Alignment = ParagraphAlignment.Center;

                            try
                            {
                                if (File.Exists(tempImagePath))
                                {
                                    File.Delete(tempImagePath);
                                }
                            }
                            catch { }
                        }
                        catch (Exception)
                        {
                            var errorPara = cell.AddParagraph();
                            errorPara.AddText("[Image could not be loaded - ProcessTableCellContent]");
                        }
                    }
                }
                else if (content.Name.Equals("figure", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle figure within table cell by using column width constraints
                    var nestedImg = content.SelectSingleNode(".//img");
                    if (nestedImg != null)
                    {
                        try
                        {
                            Paragraph imagePara = cell.AddParagraph();
                            int colIndex = cell.Column.Index;
                            Unit colWidth = cell.Table.Columns[colIndex].Width;
                            Unit maxImageWidth = colWidth - Unit.FromCentimeter(0.2);
                            AddImageToParagraph(imagePara, nestedImg, maxImageWidth);
                        }
                        catch (Exception)
                        {
                            var errorPara = cell.AddParagraph();
                            errorPara.AddText("[Image could not be loaded - Figure]");
                        }
                    }
                    // Optional caption
                    var captionNode = content.SelectSingleNode(".//figcaption");
                    if (captionNode != null && !string.IsNullOrWhiteSpace(captionNode.InnerText))
                    {
                        var captionPara = cell.AddParagraph();
                        var ft = captionPara.AddFormattedText(WebUtility.HtmlDecode(captionNode.InnerText.Trim()));
                        ft.Italic = true;
                        captionPara.Format.Alignment = ParagraphAlignment.Center;
                    }
                }
                else if (content.Name.Equals("video", StringComparison.OrdinalIgnoreCase))
                {
                    // Video not supported in PDF; render a clickable link in the cell
                    var videoUrl = content.GetAttributeValue("src", string.Empty);
                    videoUrl = HttpUtility.HtmlDecode(videoUrl);
                    videoUrl = CleanUrl(videoUrl);
                    if (!string.IsNullOrEmpty(videoUrl))
                    {
                        var linkPara = cell.AddParagraph();
                        var hyperlink = linkPara.AddHyperlink(videoUrl, HyperlinkType.Web);
                        var linkText = hyperlink.AddFormattedText($"Video: {videoUrl}");
                        linkText.Color = new Color(0, 106, 138);
                        linkText.Underline = Underline.Single;
                    }
                }
                else if (content.Name.Equals("strong", StringComparison.OrdinalIgnoreCase))
                {
                    var headingPara = cell.AddParagraph();

                    headingPara.AddText(WebUtility.HtmlDecode(content.InnerText));
                    headingPara.Format.Font.Bold = true;
                    headingPara.Format.SpaceAfter = Unit.FromPoint(6);
                    headingPara.Format.SpaceBefore = Unit.FromPoint(6);

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
                            headingPara.Format.Font.Size = 24;
                            headingPara.Format.Font.Bold = true;
                            headingPara.Format.Font.Color = new Color(28, 74, 113);
                            headingPara.Format.SpaceBefore = Unit.FromPoint(10); // IS-003: 8pt before
                            headingPara.Format.SpaceAfter = Unit.FromPoint(14);
                            break;
                        case "h2":
                            headingPara.Format.Font.Size = 18;
                            headingPara.Format.Font.Bold = true;
                            headingPara.Format.Font.Color = Colors.Black;
                            headingPara.Format.SpaceBefore = Unit.FromPoint(10); // IS-003: 8pt before
                            headingPara.Format.SpaceAfter = Unit.FromPoint(8);  // IS-003: 6pt after
                            break;
                        case "h3":
                            headingPara.Format.Font.Size = 16;
                            headingPara.Format.Font.Bold = false;
                            headingPara.Format.Font.Color = Colors.Black;
                            headingPara.Format.SpaceBefore = Unit.FromPoint(14); // 14pt padding before subheadings
                            headingPara.Format.SpaceAfter = Unit.FromPoint(8);   // 8pt after for clear separation  
                            break;
                        case "h4":
                            headingPara.Format.Font.Size = 14;
                            headingPara.Format.Font.Bold = false;
                            headingPara.Format.Font.Color = Colors.Black;
                            headingPara.Format.SpaceBefore = Unit.FromPoint(12);    
                            headingPara.Format.SpaceAfter = Unit.FromPoint(8);  // 8pt after for clear separation  
                            break;
                        default:
                            headingPara.Format.Font.Size = 12;
                            headingPara.Format.Font.Bold = false;
                            headingPara.Format.Font.Color = Colors.Black;
                            headingPara.Format.SpaceBefore = Unit.FromPoint(6);
                            headingPara.Format.SpaceAfter = Unit.FromPoint(6);
                            break;
                    }

                    // Ensure heading in table cells is not orphaned at page bottom
                    headingPara.Format.KeepWithNext = true;
                    headingPara.Format.KeepTogether = true;

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
                                    //listItemPara.Format.LeftIndent = Unit.FromCentimeter(0.02);
                                    //listItemPara.Format.FirstLineIndent = Unit.FromCentimeter(-0.02);
                                    listItemPara.AddText("• ");

                                    // Process text content in list item
                                    var liTextNodes = liNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text);
                                    foreach (var liTextNode in liTextNodes)
                                    {
                                        if (!string.IsNullOrWhiteSpace(liTextNode.InnerText))
                                        {
                                            listItemPara.AddText(WebUtility.HtmlDecode(liTextNode.InnerText.Trim()));
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

                                                FormattedText formattedText = listItemPara.AddFormattedText(WebUtility.HtmlDecode(liSpanNode.InnerText.Trim()));

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
                            listItemPara.Format.LeftIndent = Unit.FromCentimeter(0.2);
                            listItemPara.Format.FirstLineIndent = Unit.FromCentimeter(-0.2);

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
                                        listItemPara.AddText("• ");
                                        listItemPara.AddText(WebUtility.HtmlDecode(pNode.InnerText.Trim()));
                                    }
                                    else
                                    {
                                        var additionalPara = cell.AddParagraph();
                                        additionalPara.Format.LeftIndent = Unit.FromCentimeter(1.0);
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
                                        listItemPara.AddText("• ");
                                        listItemPara.AddText(WebUtility.HtmlDecode(liTextNode.InnerText.Trim()));
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

                                            FormattedText formattedText = listItemPara.AddFormattedText(WebUtility.HtmlDecode(liSpanNode.InnerText.Trim()));

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
                    // Handle paragraphs in table cells with inline formatting support
                    var paragraph = cell.AddParagraph();
                    paragraph.Format.KeepTogether = true;

                    // If the paragraph contains an image, handle that first and skip inline text
                    var imgNode = content.SelectSingleNode(".//img");
                    if (imgNode != null)
                    {
                        string imageUrl = imgNode.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            try
                            {
                                string decodedUrl = HttpUtility.HtmlDecode(imageUrl);
                                string fileName = Path.GetFileName(new Uri(decodedUrl).AbsolutePath);
                                string tempImagePath = Path.Combine(Path.GetTempPath(), fileName);

                                using (WebClient client = new WebClient())
                                {
                                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                                    client.DownloadFile(decodedUrl, tempImagePath);
                                }

                                var image = paragraph.AddImage(tempImagePath);

                                int colIndex = cell.Column.Index;
                                if (colIndex >= 0 && colIndex < cell.Table.Columns.Count)
                                {
                                    image.Width = Unit.FromCentimeter(cell.Table.Columns[colIndex].Width.Centimeter - 0.5);
                                }
                                else
                                {
                                    image.Width = Unit.FromCentimeter(5);
                                }
                                image.LockAspectRatio = true;
                            }
                            catch (Exception ex)
                            {
                                paragraph.AddText(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        // Render inline child nodes recursively to support nested tags and styles
                        AddInlineParagraphContent(paragraph, content, false, false, false, null, null);
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

        // Helper to clean media URLs (remove stray backticks/spaces)
        private string CleanUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;
            var cleaned = url.Trim().Trim('`').Trim('"');
            // Replace spaces with %20 to ensure valid URLs
            if (cleaned.IndexOf(' ') >= 0)
            {
                cleaned = cleaned.Replace(" ", "%20");
            }
            return cleaned.Trim();
        }

        private void AddHorizontalLine(Section section)
        {
            Paragraph para = section.AddParagraph();
            para.Format.Borders.Bottom.Width = 0.5;
            para.Format.Borders.Bottom.Color = new Color(28, 74, 113); // Nomadix blue color
            para.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            para.Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }

        // Recursively render inline content with inherited styles (bold/italic/underline/color/size)
        private void AddInlineParagraphContent(Paragraph paragraph, HtmlNode node,
            bool parentBold, bool parentItalic, bool parentUnderline,
            Color? parentColor, int? parentFontSize)
        {
            bool isBold = parentBold;
            bool isItalic = parentItalic;
            bool isUnderline = parentUnderline;
            Color? color = parentColor;
            int? fontSize = parentFontSize;

            // Apply tag-based style inheritance
            string nodeName = node.Name?.ToLower() ?? string.Empty;
            if (nodeName == "strong" || nodeName == "b") isBold = true;
            if (nodeName == "em" || nodeName == "i") isItalic = true;
            if (nodeName == "u") isUnderline = true;

            // Apply inline style attributes if present (e.g., span styles)
            string style = node.GetAttributeValue("style", "");
            if (!string.IsNullOrEmpty(style))
            {
                string colorValue = ExtractStyleValue(style, "color");
                if (!string.IsNullOrEmpty(colorValue))
                {
                    color = ParseColor(colorValue);
                }

                string fontSizeValue = ExtractStyleValue(style, "font-size");
                if (!string.IsNullOrEmpty(fontSizeValue))
                {
                    int sz = ParseFontSize(fontSizeValue);
                    if (sz > 0) fontSize = sz;
                }

                string fontWeightValue = ExtractStyleValue(style, "font-weight");
                if (!string.IsNullOrEmpty(fontWeightValue))
                {
                    // Treat bold, bolder, or numeric weights >= 600 as bold
                    if (fontWeightValue.Equals("bold", StringComparison.OrdinalIgnoreCase) ||
                        fontWeightValue.Equals("bolder", StringComparison.OrdinalIgnoreCase))
                    {
                        isBold = true;
                    }
                    else if (int.TryParse(fontWeightValue, out int fw) && fw >= 600)
                    {
                        isBold = true;
                    }
                }

                string textDecorationValue = ExtractStyleValue(style, "text-decoration");
                if (!string.IsNullOrEmpty(textDecorationValue) &&
                    textDecorationValue.IndexOf("underline", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    isUnderline = true;
                }
                string fontStyleValue = ExtractStyleValue(style, "font-style");
                if (!string.IsNullOrEmpty(fontStyleValue) &&
                    fontStyleValue.Equals("italic", StringComparison.OrdinalIgnoreCase))
                {
                    isItalic = true;
                }
            }

            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    string text = child.InnerText;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var ft = paragraph.AddFormattedText(WebUtility.HtmlDecode(text));
                        if (isBold) ft.Bold = true;
                        if (isItalic) ft.Italic = true;
                        if (isUnderline) ft.Underline = Underline.Single;
                        if (color.HasValue) ft.Font.Color = color.Value;
                        if (fontSize.HasValue && fontSize.Value > 0) ft.Font.Size = fontSize.Value;
                    }
                }
                else if (child.Name.Equals("br", StringComparison.OrdinalIgnoreCase))
                {
                    paragraph.AddLineBreak();
                }
                else
                {
                    // Recurse into nested inline elements to carry accumulated styles
                    AddInlineParagraphContent(paragraph, child, isBold, isItalic, isUnderline, color, fontSize);
                }
            }
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
                        image.LockAspectRatio = true;

                        // Compute usable page width (paragraph-level image)
                        section = para.Section;
                        Unit containerWidth = section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin;

                        // Default target width of 15cm, but never exceed container width
                        Unit targetWidth = Unit.FromCentimeter(15);
                        if (targetWidth > containerWidth)
                        {
                            targetWidth = containerWidth;
                        }
                        image.Width = targetWidth;

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
                        image.LockAspectRatio = true;

                        // Compute usable page width (paragraph-level image)
                        section = para.Section;
                        Unit containerWidth = section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin;

                        // Default target width of 15cm, but never exceed container width
                        Unit targetWidth = Unit.FromCentimeter(15);
                        if (targetWidth > containerWidth)
                        {
                            targetWidth = containerWidth;
                        }
                        image.Width = targetWidth;
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

    public class UrlRequestModel
    {
        public string Url { get; set; }
    }

    public class ArticleRequest
    {
        public string ArticleId { get; set; }
    }

    //public class HtmlRequestModel
    //{
    //    public string HtmlContent { get; set; }
    //}
}