using HtmlAgilityPack;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Newtonsoft.Json;
using PickupAPi.Models;
using PickupAPi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace PickupAPi.Controllers
{
    [RoutePrefix("api/BusinessPdf")]
    //[EnableCors(origins: "https://kms.cloud.global", headers: "*", methods: "*")]
    public class _BusinessPdfController : ApiController
    {
        public class UrlRequestModel
        {
            public string Url { get; set; }
            public bool? ForceRefresh { get; set; } = false;
        }

        private static string API_TOKEN = "qstNVgrrO9A6w9byiy2c/n4Cza4lkaOLmXX9KXSx6yH4/0FDSBdOeSnP40bamD7jaJegf5sb0azs9GH1aOALH1qxM74IHURyIdvhC2ijW9tmHyc5TuLG5KOYibNfRaQ0mzMIffNJzcFqff5TtUFb8w==";

        private int mainsrno = 1;
        private int subsrno = 1;
        private readonly List<(string Title, int PageNumber)> blockquoteIndex = new List<(string, int)>();
        private List<(string Title, string Bookmark, string Level)> headings;
        private bool isFirst = true;

        private const string API_BASE = "https://apihub.document360.io";
        private const string LANG_CODE = "en";

        private static readonly string PDF_CACHE_DIR = HttpContext.Current != null
            ? HttpContext.Current.Server.MapPath("~/App_Data/PdfCache")
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfCache");

        private static readonly string IMAGE_CACHE_DIR = HttpContext.Current != null
            ? HttpContext.Current.Server.MapPath("~/App_Data/PdfCache/images")
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfCache", "images");

        private List<int> headingNumbers = new List<int>();

        // Change #2: Use ArialFontResolver instead of MontserratFontResolver
        static _BusinessPdfController()
        {
            PdfSharp.Fonts.GlobalFontSettings.FontResolver = new ArialFontResolver();
        }

        // =========================================================================
        // DIAGNOSTIC ENDPOINT
        // =========================================================================

        [HttpGet]
        [Route("Diag")]
        public HttpResponseMessage Diag()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== DIAGNOSTIC ===");
            info.AppendLine("DateTime: " + DateTime.Now);
            info.AppendLine("Is64BitProcess: " + Environment.Is64BitProcess);
            info.AppendLine("CLR: " + Environment.Version);
            info.AppendLine("FontResolver: " + (PdfSharp.Fonts.GlobalFontSettings.FontResolver?.GetType().FullName ?? "NULL"));

            try
            {
                var fontResolver = new ArialFontResolver();
                byte[] fontBytes = fontResolver.GetFont("Arial#Regular");
                info.AppendLine("Arial#Regular loaded: " + (fontBytes != null ? fontBytes.Length + " bytes" : "NULL"));
            }
            catch (Exception ex)
            {
                info.AppendLine("Arial#Regular ERROR: " + ex.Message);
            }

            try
            {
                var doc = new MigraDoc.DocumentObjectModel.Document();
                var style = doc.Styles["Normal"];
                style.Font.Name = "Arial";
                style.Font.Size = 10;

                var section = doc.AddSection();
                section.PageSetup.PageWidth = Unit.FromCentimeter(21.0);
                section.PageSetup.PageHeight = Unit.FromCentimeter(29.7);

                var para = section.AddParagraph("Hello World - Diagnostic Test");
                para.Format.Font.Size = 24;

                var renderer = new DocumentRenderer(doc);
                renderer.PrepareDocument();
                info.AppendLine("DocumentRenderer.PageCount: " + renderer.FormattedDocument.PageCount);

                var pdfRenderer = new PdfDocumentRenderer(unicode: true);
                pdfRenderer.Document = doc;
                pdfRenderer.RenderDocument();

                using (var ms = new MemoryStream())
                {
                    pdfRenderer.PdfDocument.Save(ms, closeStream: false);
                    byte[] bytes = ms.ToArray();
                    info.AppendLine("PDF bytes: " + bytes.Length);

                    // Save to disk for manual inspection
                    string diagPath = HttpContext.Current.Server.MapPath("~/App_Data/PdfCache/diag_test.pdf");
                    Directory.CreateDirectory(Path.GetDirectoryName(diagPath));
                    File.WriteAllBytes(diagPath, bytes);
                    info.AppendLine("Saved to: " + diagPath);

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(bytes)
                    };
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    return response;
                }
            }
            catch (Exception ex)
            {
                info.AppendLine("PDF GENERATION ERROR: " + ex);
            }

            HttpResponseMessage textResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(info.ToString(), Encoding.UTF8, "text/plain")
            };
            return textResponse;
        }

        [HttpGet]
        [Route("DiagHtml")]
        public HttpResponseMessage DiagHtml()
        {
            string testHtml = "<h1>Test Heading 1</h1><p>This is a test paragraph with <b>bold text</b> and normal text.</p><h2>Test Heading 2</h2><p>Another paragraph under heading 2.</p><ul><li>Item 1</li><li>Item 2</li></ul>";

            try
            {
                Console.WriteLine("[DiagHtml] Generating PDF with test HTML...");
                Console.WriteLine("[DiagHtml] FontResolver type: " + (PdfSharp.Fonts.GlobalFontSettings.FontResolver?.GetType().FullName ?? "NULL"));
                Console.WriteLine("[DiagHtml] Is64BitProcess: " + Environment.Is64BitProcess);

                byte[] pdfBytes = GenerateBusinessPdf(testHtml);
                Console.WriteLine("[DiagHtml] PDF generated: " + pdfBytes.Length + " bytes");

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(pdfBytes)
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "diag_html_test.pdf"
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DiagHtml] ERROR: " + ex);
                HttpResponseMessage textResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("ERROR: " + ex.ToString(), Encoding.UTF8, "text/plain")
                };
                return textResponse;
            }
        }

        [HttpGet]
        [Route("DiagInfo")]
        public IHttpActionResult DiagInfo()
        {
            var info = new
            {
                Is64BitProcess = Environment.Is64BitProcess,
                CLRVersion = Environment.Version.ToString(),
                FontResolverType = PdfSharp.Fonts.GlobalFontSettings.FontResolver?.GetType().FullName ?? "NULL",
                OSVersion = Environment.OSVersion.ToString(),
                MachineName = Environment.MachineName
            };
            return Ok(info);
        }

        // =========================================================================
        // ENDPOINTS
        // =========================================================================

        [HttpPost]
        [Route("GeneratePdf")]
        [ResponseType(typeof(byte[]))]
        public async Task<HttpResponseMessage> GeneratePdf([FromBody] HtmlRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.htmlContent))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid HTML Content");

            try
            {
                if (PdfSharp.Fonts.GlobalFontSettings.FontResolver == null)
                    PdfSharp.Fonts.GlobalFontSettings.FontResolver = new ArialFontResolver();

                byte[] array = GenerateBusinessPdf(request.htmlContent, request.CoverPageType);

                HttpResponseMessage val = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = (HttpContent)new ByteArrayContent(array)
                };
                val.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                val.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "NomadixBusinessDocument.pdf"
                };
                return val;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.StackTrace);
            }
        }

        [HttpGet]
        [Route("test-article")]
        public async Task<IHttpActionResult> TestArticle()
        {
            string articleUrl = "https://kms.cloud.global/trustedwifi/docs/administration";
            return Ok(await GetArticleByUrl(articleUrl));
        }

        // Change #8: PDF Cache
        [HttpPost]
        [Route("GenerateFromUrl")]
        public async Task<HttpResponseMessage> GenerateFromUrl([FromBody] UrlRequestModel req)
        {
            Debug.WriteLine("[PDF-GFU] === GenerateFromUrl START ===");
            Debug.WriteLine("[PDF-GFU] URL: " + req?.Url);
            Debug.WriteLine("[PDF-GFU] ForceRefresh: " + req?.ForceRefresh);

            if (string.IsNullOrWhiteSpace(req?.Url))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "URL missing");

            string cacheFilePath = GetCacheFilePath(req.Url);
            byte[] array = req.ForceRefresh.GetValueOrDefault() ? null : TryGetCachedPdf(cacheFilePath);
            Debug.WriteLine("[PDF-GFU] Cache hit: " + (array != null));

            if (req.ForceRefresh.GetValueOrDefault() && File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
                Debug.WriteLine("[PDF-GFU] CLEARED cache: " + cacheFilePath);
            }

            if (array != null)
            {
                Debug.WriteLine("[PDF-GFU] Returning CACHED PDF (" + array.Length + " bytes)");
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(new Uri(req.Url).Segments.LastOrDefault() ?? "Document");
                return BuildPdfResponse(array, fileNameWithoutExtension);
            }

            try
            {
                Debug.WriteLine("[PDF-GFU] Fetching article from API...");
                dynamic val = JsonConvert.DeserializeObject(await GetArticleByUrl(req.Url));
                if (val?.data == null)
                {
                    Debug.WriteLine("[PDF-GFU] Article not found!");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Article not found");
                }

                string articleId = (string)val.data.id;
                string articleTitle = (string)val.data.title;
                Debug.WriteLine("[PDF-GFU] Article ID: " + articleId + ", Title: " + articleTitle);

                dynamic val2 = JsonConvert.DeserializeObject(await GetArticleDetail(articleId));
                if (val2?.data == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Article detail not found");

                string categoryName = ((string)val2.data.title) ?? Path.GetFileNameWithoutExtension(new Uri(req.Url).Segments.LastOrDefault() ?? "Document");
                string value2 = ((string)val2.data.html_content) ?? "";
                Debug.WriteLine("[PDF-GFU] Article html_len=" + (value2?.Length ?? 0));

                StringBuilder fullHtml = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(value2))
                {
                    fullHtml.Append("<h1>" + WebUtility.HtmlEncode(categoryName) + "</h1>");
                    fullHtml.Append(value2);
                }

                Debug.WriteLine("[PDF-GFU] Total HTML length: " + fullHtml.Length);
                if (fullHtml.Length == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No content retrieved");

                Debug.WriteLine("[PDF-GFU] Calling GenerateBusinessPdf...");
                byte[] pdfBytes = GenerateBusinessPdf(fullHtml.ToString());
                Debug.WriteLine("[PDF-GFU] PDF generated: " + pdfBytes.Length + " bytes");
                SavePdfToCache(cacheFilePath, pdfBytes);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(pdfBytes);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentLength = pdfBytes.Length;
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("inline")
                {
                    FileName = categoryName + ".pdf"
                };
                return response;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("[PDF-GFU] HttpRequestException: " + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Document360 API unreachable: " + ex.Message);
            }
            catch (Exception ex2)
            {
                Debug.WriteLine("[PDF-GFU] Exception: " + ex2);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex2.Message);
            }
        }

        // Change #9: Keep old endpoint
        [HttpPost]
        [Route("GenerateFromUrl_Old")]
        public async Task<HttpResponseMessage> GenerateFromUrl_Old([FromBody] UrlRequestModel req)
        {
            if (string.IsNullOrWhiteSpace(req?.Url))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "URL missing");

            try
            {
                dynamic val = JsonConvert.DeserializeObject(await GetArticleByUrl(req.Url));
                if (val?.data == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Article not found for the given URL");

                string text = (string)val.data.category_id;
                if (string.IsNullOrEmpty(text))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Article has no category_id");

                dynamic categoryObj = JsonConvert.DeserializeObject(await GetCategoryArticles(text));
                if (categoryObj?.data == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Category not found");

                List<string> list = ExtractArticleIds(categoryObj.data);
                if (!list.Any())
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No articles found in category");

                StringBuilder fullHtml = new StringBuilder();
                foreach (string articleId in list)
                {
                    try
                    {
                        object obj = JsonConvert.DeserializeObject(await GetArticleDetail(articleId));
                        string value = ((string)((dynamic)obj)?.data?.title) ?? "";
                        string value2 = ((string)((dynamic)obj)?.data?.html_content) ?? "";
                        if (!string.IsNullOrWhiteSpace(value2))
                        {
                            fullHtml.Append("<h1>" + WebUtility.HtmlEncode(value) + "</h1>");
                            fullHtml.Append(value2);
                            fullHtml.Append("<hr style='page-break-after:always;'/>");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Skipping article " + articleId + ": " + ex.Message);
                    }
                }

                if (fullHtml.Length == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No content could be retrieved");

                byte[] array = GenerateBusinessPdf(fullHtml.ToString());
                string text2 = ((string)categoryObj.data.name) ?? "Section";
                string text3 = string.Concat(text2.Split(Path.GetInvalidFileNameChars()));
                HttpResponseMessage val2 = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = (HttpContent)new ByteArrayContent(array)
                };
                val2.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                val2.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = text3 + ".pdf"
                };
                return val2;
            }
            catch (HttpRequestException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Document360 API unreachable: " + ex.Message);
            }
            catch (Exception ex2)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex2.Message);
            }
        }

        // Change #8: ClearCache endpoint
        [HttpDelete]
        [Route("ClearCache")]
        public HttpResponseMessage ClearCache([FromBody] UrlRequestModel req)
        {
            if (string.IsNullOrWhiteSpace(req?.Url))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "URL missing");

            string cacheFilePath = GetCacheFilePath(req.Url);
            if (!File.Exists(cacheFilePath))
                return Request.CreateResponse(HttpStatusCode.NotFound, "No cached PDF for this URL");

            File.Delete(cacheFilePath);
            return Request.CreateResponse(HttpStatusCode.OK, "Cache cleared");
        }

        // Change #9: Keep GenerateFullSectionPdf endpoint
        [HttpPost]
        [Route("GenerateFullSectionPdf")]
        public async Task<HttpResponseMessage> GenerateFullSectionPdf([FromBody] ArticleRequest req)
        {
            if (string.IsNullOrEmpty(req?.ArticleId))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid ArticleId");

            HttpClient client = new HttpClient();
            try
            {
                dynamic val = JsonConvert.DeserializeObject(await client.GetStringAsync("https://kms.cloud.global/api/document/get-article-detail?articleId=" + req.ArticleId + "&lang=en&version-slug=trustedwifi"));
                string text = val.data.categoryId;
                dynamic val2 = JsonConvert.DeserializeObject(await client.GetStringAsync("https://kms.cloud.global/api/document/get-category-articles?categoryId=" + text + "&lang=en&version-slug=trustedwifi"));
                dynamic val3 = val2.data.articles;
                List<object> list = ((IEnumerable<object>)val3).OrderBy((dynamic a) => (int)a.order).ToList();
                List<string> allHtml = new List<string>();

                foreach (dynamic item in list)
                {
                    dynamic val4 = JsonConvert.DeserializeObject(await client.GetStringAsync($"https://kms.cloud.global/api/document/get-article-detail?articleId={(object)item.id}&lang=en&version-slug=trustedwifi"));
                    string text2 = val4.data.title;
                    string text3 = val4.data.content;
                    if (!string.IsNullOrEmpty(text3))
                    {
                        text3 = "<h1>" + text2 + "</h1>" + text3;
                        allHtml.Add(text3);
                    }
                }

                StringBuilder stringBuilder = new StringBuilder();
                foreach (string item2 in allHtml)
                {
                    stringBuilder.Append("<div style='page-break-before:always'></div>");
                    stringBuilder.Append(item2);
                }

                byte[] array = GenerateBusinessPdf(stringBuilder.ToString());
                HttpResponseMessage val5 = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = (HttpContent)new ByteArrayContent(array)
                };
                val5.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                val5.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "FullSection.pdf"
                };
                return val5;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                ((IDisposable)client)?.Dispose();
            }
        }

        // =========================================================================
        // PDF CACHE HELPERS
        // =========================================================================

        private string GetCacheFilePath(string articleUrl)
        {
            using (SHA256 sHA = SHA256.Create())
            {
                byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(articleUrl.Trim().ToLowerInvariant()));
                string text = BitConverter.ToString(array).Replace("-", "").ToLowerInvariant();
                return Path.Combine(PDF_CACHE_DIR, text + ".pdf");
            }
        }

        private byte[] TryGetCachedPdf(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Console.WriteLine("[PDF Cache] HIT -> " + filePath);
                    return File.ReadAllBytes(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PDF Cache] Read error: " + ex.Message);
            }
            return null;
        }

        private void SavePdfToCache(string filePath, byte[] pdfBytes)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllBytes(filePath, pdfBytes);
                Console.WriteLine("[PDF Cache] SAVED -> " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PDF Cache] Write error: " + ex.Message);
            }
        }

        private HttpResponseMessage BuildPdfResponse(byte[] pdfBytes, string categoryName)
        {
            string text = string.Concat((categoryName ?? "Section").Split(Path.GetInvalidFileNameChars()));
            HttpResponseMessage val = new HttpResponseMessage(HttpStatusCode.OK);
            val.Content = new ByteArrayContent(pdfBytes);
            val.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            val.Content.Headers.ContentLength = pdfBytes.Length;
            val.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("inline")
            {
                FileName = text + ".pdf"
            };
            val.Headers.Add("X-PDF-Size", pdfBytes.Length.ToString());
            return val;
        }

        // =========================================================================
        // IMAGE CACHE (Change #5)
        // =========================================================================

        private string GetCachedImagePath(string imageUrl)
        {
            try
            {
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(imageUrl.Trim().ToLowerInvariant()));
                    string hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    string ext = Path.GetExtension(new Uri(imageUrl).AbsolutePath);
                    if (string.IsNullOrEmpty(ext)) ext = ".png";
                    return Path.Combine(IMAGE_CACHE_DIR, hex + ext);
                }
            }
            catch
            {
                return null;
            }
        }

        private string DownloadImageCached(string imageUrl)
        {
            string cachedPath = GetCachedImagePath(imageUrl);
            if (cachedPath != null && File.Exists(cachedPath))
                return cachedPath;

            try
            {
                Directory.CreateDirectory(IMAGE_CACHE_DIR);
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
                    webClient.DownloadFile(imageUrl, tempPath);

                    if (cachedPath != null)
                    {
                        File.Move(tempPath, cachedPath);
                        return cachedPath;
                    }
                    return tempPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ImageCache] Download error: " + ex.Message);
                return null;
            }
        }

        // =========================================================================
        // PDF GENERATION CORE
        // =========================================================================

        public byte[] GenerateBusinessPdf(string htmlContent, int coverPageType = 0)
        {
            Debug.WriteLine("[PDF] === GenerateBusinessPdf START ===");
            Debug.WriteLine("[PDF] HTML length: " + (htmlContent?.Length ?? 0));
            Debug.WriteLine("[PDF] HTML preview: " + (htmlContent?.Substring(0, Math.Min(200, htmlContent?.Length ?? 0)) ?? "NULL"));
            Debug.WriteLine("[PDF] FontResolver: " + (PdfSharp.Fonts.GlobalFontSettings.FontResolver?.GetType().Name ?? "NULL"));
            Debug.WriteLine("[PDF] Is64Bit: " + Environment.Is64BitProcess);

            MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();
            HtmlDocument htmlDocument = new HtmlDocument();

            if (!string.IsNullOrEmpty(htmlContent))
                htmlDocument.LoadHtml(htmlContent);

            Debug.WriteLine("[PDF] HTML nodes: " + htmlDocument.DocumentNode.ChildNodes.Count);

            DefineStyles(document);

            if (coverPageType == 1)
                AddCustomCoverPage(document);
            else
                AddCoverPage(document);

            Debug.WriteLine("[PDF] Cover page added");

            headings = new List<(string, string, string)>();
            Section indexSection = CreateAndInsertTocSection(document);

            Section section = document.AddSection();
            AddHeader(section);
            AddFooter(section);

            PageSetup pageSetup = section.PageSetup;
            pageSetup.PageWidth = Unit.FromCentimeter(21.0);
            pageSetup.PageHeight = Unit.FromCentimeter(29.7);
            pageSetup.TopMargin = Unit.FromCentimeter(2.5);
            pageSetup.BottomMargin = Unit.FromCentimeter(2.5);
            pageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            pageSetup.RightMargin = Unit.FromCentimeter(2.5);
            pageSetup.HeaderDistance = Unit.FromCentimeter(0.8);
            pageSetup.FooterDistance = Unit.FromCentimeter(0.8);

            ProcessHtmlContent(htmlContent, section);

            Debug.WriteLine("[PDF] Headings found: " + headings.Count);
            Debug.WriteLine("[PDF] Sections: " + document.Sections.Count);

            DocumentRenderer documentRenderer = new DocumentRenderer(document);
            documentRenderer.PrepareDocument();

            Debug.WriteLine("[PDF] DocumentRenderer prepared. PageCount: " + documentRenderer.FormattedDocument.PageCount);

            PopulateIndexSection(indexSection, headings);

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer(unicode: true);
            pdfDocumentRenderer.Document = document;
            pdfDocumentRenderer.RenderDocument();

            Debug.WriteLine("[PDF] RenderDocument done");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                pdfDocumentRenderer.PdfDocument.Save(memoryStream, closeStream: false);
                byte[] result = memoryStream.ToArray();
                Debug.WriteLine("[PDF] PDF bytes: " + result.Length);
                Debug.WriteLine("[PDF] === GenerateBusinessPdf END ===");
                return result;
            }
        }

        // =========================================================================
        // COVER PAGES
        // =========================================================================

        private void AddCoverPage(MigraDoc.DocumentObjectModel.Document doc)
        {
            Section section = doc.AddSection();
            section.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21.0),
                PageHeight = Unit.FromCentimeter(29.7),
                TopMargin = Unit.FromCentimeter(0.0),
                BottomMargin = Unit.FromCentimeter(0.0),
                LeftMargin = Unit.FromCentimeter(0.0),
                RightMargin = Unit.FromCentimeter(0.0)
            };

            Image image = section.AddImage(HttpContext.Current.Server.MapPath("~/logo/header-logo.png"));
            image.Width = section.PageSetup.PageWidth;
            image.Height = Unit.FromCentimeter(18.0);
            image.RelativeVertical = RelativeVertical.Page;
            image.RelativeHorizontal = RelativeHorizontal.Page;
            image.Top = Unit.FromCentimeter(0.0);
            image.Left = Unit.FromCentimeter(0.0);
            image.WrapFormat.Style = WrapStyle.Through;

            Image image2 = section.AddImage(HttpContext.Current.Server.MapPath("~/logo/middle.png"));
            image2.Width = section.PageSetup.PageWidth;
            image2.Height = Unit.FromCentimeter(7.0);
            image2.RelativeVertical = RelativeVertical.Page;
            image2.RelativeHorizontal = RelativeHorizontal.Page;
            image2.Top = Unit.FromCentimeter(19.0);
            image2.Left = Unit.FromCentimeter(0.0);
            image2.WrapFormat.Style = WrapStyle.Through;

            Table table = section.Footers.Primary.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromCentimeter(16.0));
            table.AddColumn(Unit.FromCentimeter(5.0));
            Row row = table.AddRow();
            row.Shading.Color = new MigraDoc.DocumentObjectModel.Color(122, 181, 92);
            Paragraph paragraph = row.Cells[0].AddParagraph();
            paragraph.AddFormattedText("® NOMADIX", TextFormat.Bold);
            paragraph.Format.Font.Size = 11;
            paragraph.Format.Font.Color = Colors.White;
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Format.LeftIndent = Unit.FromCentimeter(0.5);
            row.Cells[0].VerticalAlignment = VerticalAlignment.Center;
            row.Height = Unit.FromCentimeter(1.4);
        }

        private void AddCustomCoverPage(MigraDoc.DocumentObjectModel.Document doc)
        {
            Section section = doc.AddSection();
            section.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21.0),
                PageHeight = Unit.FromCentimeter(29.7),
                TopMargin = Unit.FromCentimeter(0.0),
                BottomMargin = Unit.FromCentimeter(0.0),
                LeftMargin = Unit.FromCentimeter(0.0),
                RightMargin = Unit.FromCentimeter(0.0)
            };

            Image image = section.AddImage(HttpContext.Current.Server.MapPath("~/logo/pdfbackground.jpg"));
            image.Width = section.PageSetup.PageWidth;
            image.Height = section.PageSetup.PageHeight;
            image.RelativeVertical = RelativeVertical.Page;
            image.RelativeHorizontal = RelativeHorizontal.Page;
            image.Top = Unit.FromCentimeter(0.0);
            image.Left = Unit.FromCentimeter(0.0);
            image.WrapFormat.Style = WrapStyle.Through;
        }

        // =========================================================================
        // HEADER / FOOTER
        // =========================================================================

        private void AddHeader(Section section)
        {
            HeaderFooter primary = section.Headers.Primary;
            Table table = primary.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromCentimeter(10.0));
            table.AddColumn(Unit.FromCentimeter(6.0));
            Row row = table.AddRow();
            Paragraph paragraph = row.Cells[0].AddParagraph("Administration Guide");
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Center;

            Paragraph paragraph2 = row.Cells[1].AddParagraph("Nomadix");
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.Font.Bold = true;
            paragraph2.Format.Alignment = ParagraphAlignment.Right;
            row.Cells[1].VerticalAlignment = VerticalAlignment.Center;

            Paragraph paragraph3 = primary.AddParagraph();
            paragraph3.Format.Borders.Top.Width = 0.2;
            paragraph3.Format.Borders.Top.Color = Colors.Gray;
            paragraph3.Format.SpaceAfter = Unit.FromCentimeter(0.2);
        }

        private void AddFooter(Section section)
        {
            HeaderFooter primary = section.Footers.Primary;
            Paragraph paragraph = primary.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.FromPoint(14.0);

            Paragraph paragraph2 = primary.AddParagraph();
            paragraph2.Format.Borders.Top.Width = 0.05;
            paragraph2.Format.Borders.Top.Color = Colors.LightGray;

            Table table = primary.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromCentimeter(4.0));
            table.AddColumn(Unit.FromCentimeter(10.0));
            table.AddColumn(Unit.FromCentimeter(2.0));
            Row row = table.AddRow();
            row.Height = Unit.FromCentimeter(0.8);

            string text = DateTime.Now.ToString("MMMM yyyy");
            Paragraph paragraph3 = row.Cells[0].AddParagraph();
            paragraph3.AddText(text);
            paragraph3.Format.Font.Size = 8;
            paragraph3.Format.Font.Color = Colors.Gray;
            paragraph3.Format.Font.Bold = false;
            paragraph3.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Center;

            Paragraph paragraph4 = row.Cells[1].AddParagraph();
            paragraph4.AddText("Information subject to change without notice");
            paragraph4.Format.Font.Size = 8;
            paragraph4.Format.Font.Color = Colors.Gray;
            paragraph4.Format.Font.Bold = false;
            paragraph4.Format.LeftIndent = Unit.FromCentimeter(0.01);
            paragraph4.Format.Alignment = ParagraphAlignment.Center;
            row.Cells[1].VerticalAlignment = VerticalAlignment.Center;

            Paragraph paragraph5 = row.Cells[2].AddParagraph();
            paragraph5.AddPageField();
            paragraph5.Format.Font.Size = 8;
            paragraph5.Format.Font.Color = Colors.Gray;
            paragraph5.Format.Font.Bold = false;
            paragraph5.Format.Alignment = ParagraphAlignment.Right;
            row.Cells[2].VerticalAlignment = VerticalAlignment.Center;
        }

        // =========================================================================
        // TOC (Index Section)
        // =========================================================================

        private Section CreateAndInsertTocSection(MigraDoc.DocumentObjectModel.Document doc)
        {
            Section section = new Section();
            section.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21.0),
                PageHeight = Unit.FromCentimeter(29.7),
                TopMargin = Unit.FromCentimeter(2.5),
                BottomMargin = Unit.FromCentimeter(2.5),
                LeftMargin = Unit.FromCentimeter(2.5),
                RightMargin = Unit.FromCentimeter(2.5)
            };
            doc.Sections.InsertObject(1, section);
            return section;
        }

        private void PopulateIndexSection(Section indexSection, List<(string Title, string Bookmark, string Level)> headings)
        {
            if (indexSection == null || headings == null || !headings.Any())
                return;

            indexSection.PageSetup = new PageSetup
            {
                PageWidth = Unit.FromCentimeter(21.0),
                PageHeight = Unit.FromCentimeter(29.7),
                TopMargin = Unit.FromCentimeter(2.5),
                BottomMargin = Unit.FromCentimeter(2.5),
                LeftMargin = Unit.FromCentimeter(2.5),
                RightMargin = Unit.FromCentimeter(2.5)
            };

            Paragraph paragraph = indexSection.AddParagraph("Table of Contents");
            paragraph.Format.Font.Size = 16;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Font.Color = Color.FromRgb(81, 162, 198);
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);

            Paragraph paragraph2 = indexSection.AddParagraph();
            paragraph2.Format.Borders.Bottom.Width = 1;
            paragraph2.Format.Borders.Bottom.Color = Color.FromRgb(81, 162, 198);
            paragraph2.Format.SpaceAfter = Unit.FromCentimeter(1.0);

            Table table = indexSection.AddTable();
            table.Borders.Visible = false;
            table.AddColumn(Unit.FromCentimeter(1.5));
            table.AddColumn(Unit.FromCentimeter(12.0));
            table.AddColumn(Unit.FromCentimeter(2.5));

            Row row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;
            row.Format.Font.Size = 12;
            row.Shading.Color = Color.FromRgb(240, 240, 240);
            row.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            row.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            row.Height = Unit.FromCentimeter(0.3);
            row.HeightRule = RowHeightRule.AtLeast;

            for (int i = 0; i < row.Cells.Count; i++)
            {
                Cell cell = row.Cells[i];
                cell.Format.LeftIndent = Unit.FromCentimeter(0.3);
                cell.Format.RightIndent = Unit.FromCentimeter(0.3);
                cell.Format.SpaceBefore = Unit.FromPoint(0.5);
                cell.Format.SpaceAfter = Unit.FromPoint(0.3);
                cell.VerticalAlignment = VerticalAlignment.Center;
                cell.Format.Alignment = ParagraphAlignment.Left;
            }

            row.Cells[0].AddParagraph("#");
            row.Cells[1].AddParagraph("Section");
            row.Cells[2].AddParagraph("Page");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

            int num = 1;
            int num2 = 1;

            Row row2 = table.AddRow();
            row2.Height = Unit.FromCentimeter(0.4);
            row2.HeightRule = RowHeightRule.AtLeast;
            for (int j = 0; j < row2.Cells.Count; j++)
            {
                row2.Cells[j].AddParagraph(string.Empty);
                row2.Cells[j].Borders.Visible = false;
            }

            foreach (var heading in headings)
            {
                Row row3 = table.AddRow();
                row3.Format.Font.Size = 10;

                if (heading.Level == "H2")
                {
                    num2 = 1;
                    Paragraph paragraph3 = row3.Cells[0].AddParagraph(num.ToString());
                    paragraph3.Format.Font.Bold = true;
                    paragraph3.Format.Font.Color = Color.FromRgb(81, 162, 198);

                    Paragraph paragraph4 = row3.Cells[1].AddParagraph();
                    Hyperlink hyperlink = paragraph4.AddHyperlink(heading.Bookmark, HyperlinkType.Local);
                    hyperlink.AddText(heading.Title);
                    hyperlink.Font.Color = Color.FromRgb(51, 51, 51);
                    hyperlink.Font.Underline = Underline.Single;
                    hyperlink.Font.Bold = true;
                    num++;
                }
                else if (heading.Level == "H3")
                {
                    Paragraph paragraph5 = row3.Cells[0].AddParagraph($"{num - 1}.{num2}");
                    paragraph5.Format.Font.Color = Color.FromRgb(120, 120, 120);

                    Paragraph paragraph6 = row3.Cells[1].AddParagraph();
                    paragraph6.Format.LeftIndent = Unit.FromCentimeter(0.8);
                    Hyperlink hyperlink2 = paragraph6.AddHyperlink(heading.Bookmark, HyperlinkType.Local);
                    hyperlink2.AddText(heading.Title);
                    hyperlink2.Font.Color = Color.FromRgb(80, 80, 80);
                    hyperlink2.Font.Underline = Underline.Single;
                    num2++;
                }

                Paragraph paragraph7 = row3.Cells[2].AddParagraph();
                Hyperlink hyperlink3 = paragraph7.AddHyperlink(heading.Bookmark, HyperlinkType.Local);
                hyperlink3.AddPageRefField(heading.Bookmark);
                hyperlink3.Font.Color = Color.FromRgb(81, 162, 198);
                hyperlink3.Font.Underline = Underline.Single;
                paragraph7.Format.Alignment = ParagraphAlignment.Center;

                row3.Format.SpaceBefore = Unit.FromPoint(0.5);
                row3.Format.SpaceAfter = Unit.FromPoint(6.0);
            }
        }

        // =========================================================================
        // STYLES (Change #1: Montserrat -> Arial)
        // =========================================================================

        private void DefineStyles(MigraDoc.DocumentObjectModel.Document doc)
        {
            Style style = doc.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 10;

            Style style2 = doc.Styles["Heading1"];
            style2.Font.Name = "Arial";
            style2.Font.Size = 24;
            style2.Font.Bold = true;
            style2.Font.Color = new MigraDoc.DocumentObjectModel.Color(28, 74, 113);
            style2.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(2.0);
            style2.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(2.0);

            Style style3 = doc.Styles["Heading2"];
            style3.Font.Name = "Arial";
            style3.Font.Size = 18;
            style3.Font.Bold = true;
            style3.Font.Color = Colors.Black;
            style3.ParagraphFormat.SpaceBefore = Unit.FromPoint(9.0);
            style3.ParagraphFormat.SpaceAfter = Unit.FromPoint(7.0);

            Style style4 = doc.Styles["Heading3"];
            style4.Font.Name = "Arial";
            style4.Font.Size = 16;
            style4.Font.Bold = false;
            style4.Font.Color = Colors.Black;
            style4.ParagraphFormat.SpaceBefore = Unit.FromPoint(14.0);
            style4.ParagraphFormat.SpaceAfter = Unit.FromPoint(8.0);

            Style style5 = doc.Styles.AddStyle("NoteBox", "Normal");
            style5.ParagraphFormat.Borders.Width = 0.5;
            style5.ParagraphFormat.Borders.Color = new MigraDoc.DocumentObjectModel.Color(0, 106, 138);
            style5.ParagraphFormat.Borders.Distance = 3;
            style5.ParagraphFormat.Shading.Color = new MigraDoc.DocumentObjectModel.Color(28, 74, 113);
            style5.ParagraphFormat.LeftIndent = 9;
            style5.ParagraphFormat.RightIndent = 9;
            style5.Font.Color = Colors.White;
            style5.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.6);
            style5.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.4);

            Style style6 = doc.Styles.AddStyle("WarningBox", "Normal");
            style6.ParagraphFormat.Borders.Width = 0.5;
            style6.ParagraphFormat.Borders.Color = new MigraDoc.DocumentObjectModel.Color(127, 100, 22);
            style6.ParagraphFormat.Borders.Distance = 3;
            style6.ParagraphFormat.Shading.Color = new MigraDoc.DocumentObjectModel.Color(253, 242, 206);
            style6.ParagraphFormat.LeftIndent = 9;
            style6.ParagraphFormat.RightIndent = 9;
            style6.Font.Color = new MigraDoc.DocumentObjectModel.Color(127, 100, 22);
            style6.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(0.6);
            style6.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.4);

            Style style7 = doc.Styles.AddStyle("Table", "Normal");
            style7.Font.Name = "Arial";
            style7.Font.Size = 10;
            style7.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(1.0);
            style7.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.8);
            style7.ParagraphFormat.LineSpacing = Unit.FromPoint(12.0);

            Style style8 = doc.Styles.AddStyle("TableHeader", "Table");
            style8.Font.Bold = true;
            style8.Font.Name = "Arial";
            style8.Font.Size = 10;
            style8.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            style8.ParagraphFormat.Shading.Color = new MigraDoc.DocumentObjectModel.Color(191, 191, 191);
        }

        // =========================================================================
        // HTML PROCESSING
        // =========================================================================

        private void ProcessHtmlContent(string htmlContent, Section section)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            HtmlNode htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//article");
            if (htmlNode != null)
                ProcessHtmlNode(htmlNode, section);
            else
                ProcessHtmlNode(htmlDocument.DocumentNode, section);
        }

        private void ProcessHtmlNode(HtmlNode node, Section section)
        {
            if (node == null) return;

            foreach (HtmlNode item in node.ChildNodes)
            {
                if (item.NodeType == HtmlNodeType.Element && item.GetAttributeValue("id", "") == "download-this-guide⬇")
                    continue;

                if (item.NodeType == HtmlNodeType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(item.InnerText))
                        section.AddParagraph(item.InnerText.Trim());
                }
                else if (item.NodeType == HtmlNodeType.Element)
                {
                    switch (item.Name.ToLower())
                    {
                        case "h1":
                            AddHeading(section, item.InnerText, "Heading1", 24);
                            break;
                        case "h2":
                            AddHeading(section, item.InnerText, "Heading2", 18);
                            subsrno++;
                            break;
                        case "h3":
                            AddHeading(section, item.InnerText, "Heading3", 16);
                            subsrno = 1;
                            mainsrno++;
                            break;
                        case "h4":
                            AddHeading(section, item.InnerText, "Heading4", 14);
                            subsrno = 1;
                            mainsrno++;
                            break;
                        case "p":
                            AddParagraph(section, item);
                            break;
                        case "blockquote":
                            ProcessBlockquote(section, item);
                            break;
                        case "ul":
                            AddList(section, item, isOrdered: false);
                            break;
                        case "ol":
                            AddList(section, item, isOrdered: true);
                            break;
                        case "table":
                            AddTable(section, item);
                            break;
                        case "hr":
                            AddHorizontalLine(section);
                            break;
                        case "img":
                            AddImage(section, item);
                            break;
                        case "div":
                        case "article":
                        case "section":
                            ProcessHtmlNode(item, section);
                            break;
                    }
                }
            }
        }

        private void ProcessBlockquote(Section section, HtmlNode item)
        {
            string iconPath = null;
            string innerTextTrimmed = item.InnerText.Trim();

            if (innerTextTrimmed.IndexOf("Notes") >= 0)
            {
                iconPath = HttpContext.Current.Server.MapPath("~/logo/info_icon.png");
                AddStyledSection(section, "Note", item.InnerText.Trim(),
                    Color.FromRgb(221, 247, 255), Color.FromRgb(28, 74, 113), iconPath);
            }
            else if (innerTextTrimmed.IndexOf("Warning") >= 0)
            {
                iconPath = HttpContext.Current.Server.MapPath("~/logo/warning_icon.png");
                AddStyledSection(section, "Warning", item.InnerText.Trim(),
                    Color.FromRgb(253, 242, 206), Color.FromRgb(127, 100, 22), iconPath);
            }
            else if (innerTextTrimmed.IndexOf("Tip") >= 0)
            {
                iconPath = HttpContext.Current.Server.MapPath("~/logo/tip_icon.png");
                AddStyledSection(section, "Tip", item.InnerText.Trim(),
                    Color.FromRgb(139, 195, 74), Colors.White, iconPath);
            }
        }

        private void AddStyledSection(Section section, string title, string content, Color backgroundColor, Color textColor, string iconPath = null)
        {
            Table table = section.AddTable();
            table.Borders.Width = 0;
            table.Format.SpaceBefore = Unit.FromCentimeter(0.05);
            table.Format.SpaceAfter = Unit.FromCentimeter(0.05);
            table.AddColumn(Unit.FromCentimeter(16.0));
            Row row = table.AddRow();
            row.HeightRule = RowHeightRule.AtLeast;
            row.Height = Unit.FromCentimeter(1.2);
            row.Shading.Color = backgroundColor;
            row.Format.SpaceBefore = Unit.FromCentimeter(0.06);
            row.Format.SpaceAfter = Unit.FromCentimeter(0.06);

            Color textColorToUse = Colors.Black;
            if (title == "Note")
                textColorToUse = Color.FromRgb(28, 74, 113);
            else if (title == "Warning")
                textColorToUse = Color.FromRgb(127, 100, 22);

            string defaultIconPath = GetDefaultIconPath(title);
            string resolvedIconPath = !string.IsNullOrEmpty(iconPath) ? iconPath : defaultIconPath;

            Cell cell = row.Cells[0];
            cell.Format.SpaceBefore = Unit.FromCentimeter(0.35);
            cell.Format.SpaceAfter = Unit.FromCentimeter(0.25);
            cell.VerticalAlignment = VerticalAlignment.Top;
            cell.Format.LeftIndent = Unit.FromCentimeter(0.4);

            string content2 = content.Replace("ℹ\ufe0f", "").Replace("⚠\ufe0f", "").Replace("\ud83d\udca1", "");

            Paragraph paragraph = cell.AddParagraph();
            string displayTitle = GetDisplayTitle(title);
            paragraph.AddFormattedText(displayTitle, TextFormat.Bold);
            paragraph.Format.Font.Color = textColor;
            paragraph.Format.Font.Size = 11;
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            string cleanedContent = CleanContent(content2, title);
            Paragraph paragraph2 = cell.AddParagraph();
            if (cleanedContent.Contains("<li>") || cleanedContent.Contains("<ul>"))
            {
                ProcessListContent(cell, cleanedContent, textColor);
            }
            else
            {
                paragraph2.AddText(cleanedContent);
                paragraph2.Format.Font.Color = textColor;
                paragraph2.Format.Font.Size = 10;
                paragraph2.Format.LineSpacing = Unit.FromCentimeter(0.25);
                paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.25);
            }

            Paragraph paragraph3 = section.AddParagraph();
            paragraph3.Format.SpaceBefore = Unit.FromCentimeter(0.01);
            paragraph3.Format.SpaceAfter = Unit.FromCentimeter(0.01);
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

        // Change #3: Fix switch expression -> C# 7.3 compatible switch statement
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
            string text = content;
            switch (title.ToLower())
            {
                case "note":
                case "notes":
                    text = text.Replace("Notes:", "").Replace("Note:", "").Replace("Note", "").Trim();
                    break;
                case "warning":
                    text = text.Replace("Warning:", "").Replace("Warning", "").Trim();
                    break;
                case "tip":
                    text = text.Replace("Tip:", "").Replace("Tip", "").Trim();
                    break;
            }
            return text;
        }

        private void ProcessListContent(Cell contentCell, string mainContent, Color textColor)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(mainContent);

            IEnumerable<string> enumerable = from n in htmlDocument.DocumentNode.ChildNodes
                                             where n.NodeType == HtmlNodeType.Text
                                             select n.InnerText.Trim() into t
                                             where !string.IsNullOrWhiteSpace(t)
                                             select t;

            foreach (string item in enumerable)
            {
                Paragraph paragraph = contentCell.AddParagraph();
                paragraph.AddText(item);
                paragraph.Format.Font.Color = textColor;
                paragraph.Format.Font.Size = 10;
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);
            }

            HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//ul");
            if (htmlNodeCollection == null) return;

            foreach (HtmlNode item2 in htmlNodeCollection)
            {
                HtmlNodeCollection liNodes = item2.SelectNodes("./li");
                if (liNodes == null) continue;

                foreach (HtmlNode item3 in liNodes)
                {
                    Paragraph paragraph2 = contentCell.AddParagraph();
                    paragraph2.AddText("• ");

                    foreach (HtmlNode item4 in item3.ChildNodes)
                    {
                        if (item4.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (HtmlNode item5 in item4.ChildNodes)
                            {
                                if (item5.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                                {
                                    string src = HttpUtility.HtmlDecode(item5.GetAttributeValue("src", ""));
                                    if (!string.IsNullOrEmpty(src))
                                        AddImageToParaSection(src, contentCell.Section);
                                }
                                else
                                {
                                    paragraph2.AddText(WebUtility.HtmlDecode(item5.InnerText));
                                }
                            }
                        }
                        else if (item4.Name.Equals("div", StringComparison.OrdinalIgnoreCase))
                        {
                            HtmlNodeCollection tables = item4.SelectNodes(".//table");
                            if (tables != null)
                            {
                                foreach (HtmlNode item6 in tables)
                                    AddTableToDocument(paragraph2, item6);
                            }
                        }
                        else if (item4.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                        {
                            string src2 = HttpUtility.HtmlDecode(item4.GetAttributeValue("src", ""));
                            if (!string.IsNullOrEmpty(src2))
                                AddImageToParaSection(src2, contentCell.Section);
                        }
                        else
                        {
                            paragraph2.AddText(WebUtility.HtmlDecode(item4.InnerText));
                        }
                    }

                    paragraph2.Format.Font.Color = textColor;
                    paragraph2.Format.Font.Size = 10;
                    paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.05);
                    paragraph2.Format.SpaceAfter = Unit.FromCentimeter(0.05);
                }
            }
        }

        // =========================================================================
        // HEADING / PARAGRAPH / IMAGE / LIST
        // =========================================================================

        private void AddHeading(Section section, string text, string style, int fontSize)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            Paragraph paragraph = section.AddParagraph();
            paragraph.Style = style;
            paragraph.Format.Font.Size = (isFirst ? 24 : fontSize);

            if (style == "Heading1" || style == "Heading2")
                paragraph.Format.Font.Bold = true;
            else
                paragraph.Format.Font.Bold = false;

            if (style == "Heading1")
                paragraph.Format.Font.Color = new MigraDoc.DocumentObjectModel.Color(28, 74, 113);
            else
                paragraph.Format.Font.Color = Colors.Black;

            paragraph.Format.KeepWithNext = true;
            paragraph.Format.KeepTogether = true;

            string text3 = WebUtility.HtmlDecode(StripHtml(text)).Trim();

            if (style == "Heading2" || style == "Heading3")
            {
                string level = (style == "Heading2") ? "H2" : "H3";
                string bookmark = level.ToLower() + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                paragraph.AddBookmark(bookmark);
                if (headings == null)
                    headings = new List<(string, string, string)>();
                headings.Add((text3, bookmark, level));
            }

            paragraph.AddText(text3);
            isFirst = false;
        }

        private void AddParagraph(Section section, HtmlNode node)
        {
            if (node != null)
            {
                Paragraph paragraph = section.AddParagraph();
                paragraph.Format.KeepTogether = true;
                paragraph.Format.KeepWithNext = false;
                ProcessInlineElements(paragraph, node);
            }
        }

        // Change #5: Use image cache
        private void AddImageToParaSection(string imageUrl, Section section)
        {
            string localPath = null;
            bool downloadedByUs = false;
            try
            {
                string url = HttpUtility.HtmlDecode(imageUrl);
                url = CleanUrl(url);

                localPath = DownloadImageCached(url);
                if (localPath == null) return;

                Unit unit;
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(localPath))
                {
                    double value = (double)image.Width / (double)image.HorizontalResolution * 72.0;
                    unit = Unit.FromPoint(value);
                }

                Paragraph paragraph = section.AddParagraph();
                Image image2 = paragraph.AddImage(localPath);
                image2.LockAspectRatio = true;
                paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);

                Unit pageWidth = section.PageSetup.PageWidth;
                Unit leftMargin = section.PageSetup.LeftMargin;
                Unit rightMargin = section.PageSetup.RightMargin;
                Unit unit2 = pageWidth - leftMargin - rightMargin;
                double value2 = Math.Min(unit.Point, unit2.Point);
                image2.Width = Unit.FromPoint(value2);
                paragraph.Format.Alignment = ParagraphAlignment.Center;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing image: " + ex.Message);
            }
        }

        private void ProcessInlineElements(Paragraph para, HtmlNode node, string titleText = null)
        {
            if (titleText == "" && node == null) return;

            if (titleText != null && !string.IsNullOrWhiteSpace(titleText.Trim()))
            {
                string path = titleText.Trim().Equals("Note", StringComparison.OrdinalIgnoreCase)
                    ? "~/logo/info_icon.png"
                    : "~/logo/warning_icon.png";
                string iconPath = HttpContext.Current.Server.MapPath(path);
                if (File.Exists(iconPath))
                {
                    Image image = para.AddImage(iconPath);
                    image.Width = Unit.FromCentimeter(0.6);
                    image.LockAspectRatio = true;
                    image.WrapFormat.Style = WrapStyle.Through;
                    image.Top = ShapePosition.Top;
                    image.Left = ShapePosition.Left;
                    para.AddText(" ");
                }
            }

            if (titleText != null)
            {
                para.AddText(titleText);
                para.AddLineBreak();
            }

            foreach (HtmlNode item in node.ChildNodes)
            {
                if (item.NodeType == HtmlNodeType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(item.InnerText))
                    {
                        para.AddText(WebUtility.HtmlDecode(item.InnerText));
                        para.Format.SpaceBefore = Unit.FromPoint(6.0);
                        para.Format.SpaceAfter = Unit.FromPoint(6.0);
                    }
                }
                else if (item.NodeType == HtmlNodeType.Element)
                {
                    if (item.Name == "div" && item.HasClass("table-shadow-wrapper"))
                    {
                        HtmlNode htmlNode = item.SelectSingleNode(".//table");
                        if (htmlNode != null)
                            AddTableToDocument(para, htmlNode);
                        continue;
                    }

                    switch (item.Name.ToLower())
                    {
                        case "b":
                        case "strong":
                            FormattedText formattedText5 = para.AddFormattedText(WebUtility.HtmlDecode(item.InnerText));
                            formattedText5.Bold = true;
                            para.Format.SpaceBefore = Unit.FromPoint(6.0);
                            para.Format.SpaceAfter = Unit.FromPoint(6.0);
                            break;
                        case "i":
                        case "em":
                            FormattedText formattedText4 = para.AddFormattedText(WebUtility.HtmlDecode(item.InnerText));
                            formattedText4.Italic = true;
                            break;
                        case "u":
                            FormattedText formattedText3 = para.AddFormattedText(WebUtility.HtmlDecode(item.InnerText));
                            formattedText3.Underline = Underline.Single;
                            break;
                        case "a":
                            FormattedText formattedText2 = para.AddFormattedText(WebUtility.HtmlDecode(item.InnerText));
                            formattedText2.Color = new MigraDoc.DocumentObjectModel.Color(0, 106, 138);
                            formattedText2.Underline = Underline.Single;
                            break;
                        case "ul":
                            AddListNoSection(para, item, isOrdered: false);
                            break;
                        case "p":
                        case "div":
                        case "span":
                            ProcessInlineElements(para, item);
                            break;
                        case "figure":
                            HtmlNode htmlNode2 = item.SelectSingleNode(".//img");
                            if (htmlNode2 != null)
                            {
                                string text3 = HttpUtility.HtmlDecode(htmlNode2.GetAttributeValue("src", ""));
                                if (!string.IsNullOrEmpty(text3))
                                    AddImageToParaSection(text3, para.Section);
                            }
                            break;
                        case "br":
                            para.AddLineBreak();
                            break;
                        case "img":
                            string attributeValue2 = item.GetAttributeValue("src", string.Empty);
                            attributeValue2 = HttpUtility.HtmlDecode(attributeValue2);
                            attributeValue2 = CleanUrl(attributeValue2);
                            if (!string.IsNullOrEmpty(attributeValue2))
                                AddImageToParaSection(attributeValue2, para.Section);
                            break;
                        case "video":
                            string attributeValue = item.GetAttributeValue("src", string.Empty);
                            attributeValue = HttpUtility.HtmlDecode(attributeValue);
                            attributeValue = CleanUrl(attributeValue);
                            if (!string.IsNullOrEmpty(attributeValue))
                            {
                                Hyperlink hyperlink = para.AddHyperlink(attributeValue, HyperlinkType.Web);
                                FormattedText formattedText = hyperlink.AddFormattedText("Click here to view Video");
                                formattedText.Color = new MigraDoc.DocumentObjectModel.Color(0, 106, 138);
                                formattedText.Underline = Underline.Single;
                                para.AddLineBreak();
                            }
                            break;
                        default:
                            if (!string.IsNullOrWhiteSpace(item.InnerText))
                            {
                                para.AddText(WebUtility.HtmlDecode(item.InnerText));
                                para.Format.SpaceBefore = Unit.FromPoint(6.0);
                                para.Format.SpaceAfter = Unit.FromPoint(6.0);
                            }
                            break;
                    }
                }
            }
        }

        // =========================================================================
        // TABLES (production proven logic)
        // =========================================================================

        private void AddTableToDocument(Paragraph para, HtmlNode tableNode)
        {
            Section section = para.Section;
            Table table = section.AddTable();
            table.Borders.Width = 0.5;

            HtmlNode htmlNode = tableNode.SelectSingleNode(".//tr");
            if (htmlNode == null) return;

            HtmlNodeCollection htmlNodeCollection = htmlNode.SelectNodes("./th|./td");
            if (htmlNodeCollection == null || htmlNodeCollection.Count == 0) return;

            int count = htmlNodeCollection.Count;
            double[] array = CalculateColumnWidths(tableNode, count);

            for (int i = 0; i < count; i++)
                table.AddColumn(Unit.FromCentimeter(array[i]));

            HtmlNodeCollection htmlNodeCollection2 = tableNode.SelectNodes(".//tr");
            if (htmlNodeCollection2 == null) return;

            int rowIndex = 0;
            foreach (HtmlNode item in htmlNodeCollection2)
            {
                Row row = table.AddRow();
                HtmlNodeCollection htmlNodeCollection3 = item.SelectNodes("./th|./td");
                if (htmlNodeCollection3 == null) continue;

                for (int j = 0; j < htmlNodeCollection3.Count && j < table.Columns.Count; j++)
                {
                    Cell cell = row.Cells[j];
                    Paragraph paragraph = cell.AddParagraph();

                    foreach (HtmlNode item2 in htmlNodeCollection3[j].ChildNodes)
                    {
                        if (item2.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                        {
                            int index = cell.Column.Index;
                            Unit width = cell.Table.Columns[index].Width;
                            AddImageToParagraph(paragraph, item2, width - Unit.FromCentimeter(0.2));
                        }
                        else if (item2.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (HtmlNode item3 in item2.ChildNodes)
                            {
                                if (item3.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        int index2 = cell.Column.Index;
                                        Unit width2 = cell.Table.Columns[index2].Width;
                                        AddImageToParagraph(paragraph, item3, width2 - Unit.FromCentimeter(0.2));
                                    }
                                    catch (Exception ex)
                                    {
                                        paragraph.AddText("[Image could not be loaded - ]" + ex.Message);
                                    }
                                }
                                else if (item3.Name.Equals("strong", StringComparison.OrdinalIgnoreCase) || item3.Name.Equals("b", StringComparison.OrdinalIgnoreCase))
                                {
                                    FormattedText formattedText = paragraph.AddFormattedText(WebUtility.HtmlDecode(item3.InnerText));
                                    formattedText.Bold = true;
                                }
                                else if (item3.Name.Equals("em", StringComparison.OrdinalIgnoreCase) || item3.Name.Equals("i", StringComparison.OrdinalIgnoreCase))
                                {
                                    FormattedText formattedText2 = paragraph.AddFormattedText(WebUtility.HtmlDecode(item3.InnerText));
                                    formattedText2.Italic = true;
                                }
                                else if (item3.Name.Equals("u", StringComparison.OrdinalIgnoreCase))
                                {
                                    FormattedText formattedText3 = paragraph.AddFormattedText(WebUtility.HtmlDecode(item3.InnerText));
                                    formattedText3.Underline = Underline.Single;
                                }
                                else if (item3.NodeType == HtmlNodeType.Text)
                                {
                                    string innerText = item3.InnerText;
                                    if (!string.IsNullOrWhiteSpace(innerText))
                                        paragraph.AddText(WebUtility.HtmlDecode(innerText));
                                }
                                else
                                {
                                    string innerText2 = item3.InnerText;
                                    if (!string.IsNullOrWhiteSpace(innerText2))
                                        paragraph.AddText(WebUtility.HtmlDecode(innerText2));
                                }
                            }
                            paragraph.Format.SpaceAfter = Unit.FromPoint(6.0);
                            paragraph.Format.SpaceBefore = Unit.FromPoint(6.0);
                        }
                        else
                        {
                            if (rowIndex == 0)
                            {
                                FormattedText formattedText6 = paragraph.AddFormattedText(WebUtility.HtmlDecode(item2.InnerText));
                                formattedText6.Bold = true;
                            }
                            else
                            {
                                paragraph.AddText(WebUtility.HtmlDecode(item2.InnerText));
                            }
                            paragraph.Format.SpaceAfter = Unit.FromPoint(6.0);
                            paragraph.Format.SpaceBefore = Unit.FromPoint(6.0);
                        }
                    }
                }
                rowIndex++;
            }
        }

        private void AddImageToParagraph(Paragraph para, HtmlNode imgNode, Unit? containerWidthOverride = null)
        {
            string attributeValue = imgNode.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(attributeValue)) return;

            try
            {
                string url = HttpUtility.HtmlDecode(attributeValue);
                url = CleanUrl(url);

                // Change #5: Use image cache
                string localPath = DownloadImageCached(url);
                if (localPath == null) return;

                Image image = para.AddImage(localPath);
                image.LockAspectRatio = true;
                para.Format.SpaceBefore = Unit.FromCentimeter(0.5);
                para.Format.SpaceAfter = Unit.FromCentimeter(0.5);

                Unit unit;
                if (containerWidthOverride.HasValue)
                {
                    unit = containerWidthOverride.Value;
                }
                else
                {
                    Section section = para.Section;
                    Unit pageWidth = section.PageSetup.PageWidth;
                    Unit leftMargin = section.PageSetup.LeftMargin;
                    Unit rightMargin = section.PageSetup.RightMargin;
                    unit = pageWidth - leftMargin - rightMargin;
                }

                Unit unit2 = Unit.FromCentimeter(15.0);
                if (unit2 > unit)
                    unit2 = unit;
                image.Width = unit2;
                para.Format.Alignment = ParagraphAlignment.Center;
            }
            catch (Exception ex)
            {
                para.AddText("[Image could not be loaded] " + ex.Message);
            }
        }

        private void AddListNoSection(Paragraph para, HtmlNode node, bool isOrdered)
        {
            if (node == null) return;

            int num = 1;
            foreach (HtmlNode item in node.SelectNodes("./li"))
            {
                Paragraph paragraph = para.Section.AddParagraph();
                paragraph.Format.LeftIndent = Unit.FromCentimeter(1.0);
                paragraph.Format.FirstLineIndent = Unit.FromCentimeter(-0.2);
                paragraph.Format.SpaceBefore = Unit.FromPoint(6.0);
                paragraph.Format.SpaceAfter = Unit.FromPoint(6.0);
                paragraph.Format.LineSpacing = Unit.FromPoint(10.0);

                if (isOrdered)
                {
                    paragraph.AddText($"{num}. ");
                    num++;
                }
                else
                {
                    paragraph.AddText("• ");
                }

                HtmlNodeCollection htmlNodeCollection = item.SelectNodes(".//p");
                if (htmlNodeCollection != null && htmlNodeCollection.Count > 0)
                {
                    foreach (HtmlNode item2 in htmlNodeCollection)
                    {
                        IEnumerable<HtmlNode> enumerable = item2.ChildNodes.Where((HtmlNode n) => n.NodeType == HtmlNodeType.Text);
                        foreach (HtmlNode item3 in enumerable)
                        {
                            if (!string.IsNullOrWhiteSpace(item3.InnerText))
                                paragraph.AddText(WebUtility.HtmlDecode(item3.InnerText.Trim()));
                        }

                        HtmlNodeCollection brNodes = item2.SelectNodes(".//br");
                        if (brNodes != null)
                        {
                            foreach (HtmlNode item4 in brNodes)
                                paragraph.AddLineBreak();
                        }

                        HtmlNodeCollection imgNodes = item2.SelectNodes(".//img");
                        if (imgNodes != null)
                        {
                            foreach (HtmlNode item5 in imgNodes)
                            {
                                string src = HttpUtility.HtmlDecode(item5.GetAttributeValue("src", string.Empty));
                                if (!string.IsNullOrEmpty(src))
                                    AddImageToParaSection(src, paragraph.Section);
                            }
                        }
                    }
                }
                else
                {
                    ProcessInlineElements(paragraph, item);
                }
            }
        }

        private void AddList(Section section, HtmlNode node, bool isOrdered)
        {
            if (node == null) return;

            int num = 1;
            foreach (HtmlNode item in node.SelectNodes("./li"))
            {
                Paragraph paragraph = section.AddParagraph();
                paragraph.Format.LeftIndent = Unit.FromCentimeter(1.0);
                paragraph.Format.FirstLineIndent = Unit.FromCentimeter(-0.2);
                paragraph.Format.SpaceBefore = Unit.FromPoint(6.0);
                paragraph.Format.SpaceAfter = Unit.FromPoint(6.0);
                paragraph.Format.LineSpacing = Unit.FromPoint(10.0);

                if (isOrdered)
                {
                    paragraph.AddText($"{num}. ");
                    num++;
                }
                else
                {
                    paragraph.AddText("• ");
                }

                ProcessInlineElements(paragraph, item);
            }
        }

        private static bool HasBoldStyle(HtmlNode node)
        {
            string text = node.GetAttributeValue("style", "")?.ToLowerInvariant();
            if (string.IsNullOrEmpty(text)) return false;
            if (text.Contains("font-weight:"))
            {
                if (!text.Contains("bold") && !text.Contains("700") && !text.Contains("800"))
                    return text.Contains("900");
                return true;
            }
            return false;
        }

        private void AddTable(Section section, HtmlNode node)
        {
            if (node == null) return;

            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.PageBreakBefore = true;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            HtmlNode htmlNode = node.SelectSingleNode(".//tr");
            if (htmlNode == null) return;

            HtmlNodeCollection htmlNodeCollection = htmlNode.SelectNodes("./th|./td");
            if (htmlNodeCollection == null || htmlNodeCollection.Count == 0) return;

            int count = htmlNodeCollection.Count;
            double[] array = CalculateColumnWidths(node, count);
            for (int i = 0; i < count; i++)
                table.AddColumn(Unit.FromCentimeter(array[i]));

            HtmlNodeCollection htmlNodeCollection2 = node.SelectNodes(".//tr");
            if (htmlNodeCollection2 == null) return;

            bool isFirstRow = true;
            foreach (HtmlNode item in htmlNodeCollection2)
            {
                if (IsBlankRow(item)) continue;

                Row row = table.AddRow();
                HtmlNodeCollection htmlNodeCollection3 = item.SelectNodes("./th|./td");
                if (htmlNodeCollection3 == null) continue;

                for (int j = 0; j < htmlNodeCollection3.Count && j < table.Columns.Count; j++)
                {
                    HtmlNode htmlNode2 = htmlNodeCollection3[j];
                    Cell cell = row.Cells[j];

                    if (isFirstRow || htmlNode2.Name.Equals("th", StringComparison.OrdinalIgnoreCase))
                    {
                        cell.Shading.Color = new MigraDoc.DocumentObjectModel.Color(191, 191, 191);
                        cell.Format.Font.Bold = true;
                        cell.Format.SpaceBefore = Unit.FromPoint(6.0);
                        cell.Format.SpaceAfter = Unit.FromPoint(6.0);
                        cell.Format.LeftIndent = Unit.FromPoint(4.0);
                        cell.Format.RightIndent = Unit.FromPoint(4.0);
                    }
                    else
                    {
                        cell.Format.SpaceBefore = Unit.FromPoint(4.0);
                        cell.Format.SpaceAfter = Unit.FromPoint(4.0);
                        cell.Format.LeftIndent = Unit.FromPoint(4.0);
                        cell.Format.RightIndent = Unit.FromPoint(4.0);
                    }

                    Paragraph paragraph2 = cell.AddParagraph();
                    if (isFirstRow)
                        paragraph2.Format.Font.Bold = true;

                    foreach (HtmlNode item2 in htmlNode2.ChildNodes)
                    {
                        if (item2.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                        {
                            int index = cell.Column.Index;
                            Unit width = cell.Table.Columns[index].Width;
                            AddImageToParagraph(paragraph2, item2, width - Unit.FromCentimeter(0.2));
                        }
                        else if (item2.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (HtmlNode item3 in item2.ChildNodes)
                            {
                                if (item3.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        int index2 = cell.Column.Index;
                                        Unit width2 = cell.Table.Columns[index2].Width;
                                        AddImageToParagraph(paragraph2, item3, width2 - Unit.FromCentimeter(0.2));
                                    }
                                    catch (Exception ex)
                                    {
                                        paragraph2.AddText("[Image could not be loaded - ]" + ex.Message);
                                    }
                                }
                                else if (item3.Name.Equals("strong", StringComparison.OrdinalIgnoreCase) || item3.Name.Equals("b", StringComparison.OrdinalIgnoreCase))
                                {
                                    FormattedText formattedText = paragraph2.AddFormattedText(WebUtility.HtmlDecode(item3.InnerText));
                                    formattedText.Bold = true;
                                }
                                else if (item3.Name.Equals("em", StringComparison.OrdinalIgnoreCase) || item3.Name.Equals("i", StringComparison.OrdinalIgnoreCase))
                                {
                                    FormattedText formattedText2 = paragraph2.AddFormattedText(WebUtility.HtmlDecode(item3.InnerText));
                                    formattedText2.Italic = true;
                                }
                                else if (item3.Name.Equals("u", StringComparison.OrdinalIgnoreCase))
                                {
                                    FormattedText formattedText3 = paragraph2.AddFormattedText(WebUtility.HtmlDecode(item3.InnerText));
                                    formattedText3.Underline = Underline.Single;
                                }
                                else if (item3.NodeType == HtmlNodeType.Text)
                                {
                                    string innerText = item3.InnerText;
                                    if (!string.IsNullOrWhiteSpace(innerText))
                                        paragraph2.AddText(WebUtility.HtmlDecode(innerText));
                                }
                                else
                                {
                                    string innerText2 = item3.InnerText;
                                    if (!string.IsNullOrWhiteSpace(innerText2))
                                        paragraph2.AddText(WebUtility.HtmlDecode(innerText2));
                                }
                            }
                            paragraph2.Format.SpaceBefore = Unit.FromPoint(6.0);
                            paragraph2.Format.SpaceAfter = Unit.FromPoint(6.0);
                        }
                        else if (item2.NodeType == HtmlNodeType.Text)
                        {
                            string innerText3 = item2.InnerText;
                            if (!string.IsNullOrWhiteSpace(innerText3))
                                paragraph2.AddText(WebUtility.HtmlDecode(innerText3));
                            paragraph2.Format.SpaceBefore = Unit.FromPoint(6.0);
                            paragraph2.Format.SpaceAfter = Unit.FromPoint(6.0);
                        }
                        else
                        {
                            string innerText4 = item2.InnerText;
                            if (!string.IsNullOrWhiteSpace(innerText4))
                                paragraph2.AddText(WebUtility.HtmlDecode(innerText4));
                            paragraph2.Format.SpaceBefore = Unit.FromPoint(6.0);
                            paragraph2.Format.SpaceAfter = Unit.FromPoint(6.0);
                        }
                    }
                }
                isFirstRow = false;
            }
        }

        // =========================================================================
        // IMAGE HELPERS
        // =========================================================================

        private void AddImage(Section section, HtmlNode node)
        {
            string attributeValue = node.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(attributeValue)) return;

            try
            {
                Paragraph paragraph = section.AddParagraph();
                paragraph.Format.Alignment = ParagraphAlignment.Center;

                if (attributeValue.StartsWith("http") || attributeValue.StartsWith("https"))
                {
                    // Change #5: Use image cache
                    string localPath = DownloadImageCached(attributeValue);
                    if (localPath == null) return;

                    Image image = paragraph.AddImage(localPath);
                    image.LockAspectRatio = true;
                    Section sec = paragraph.Section;
                    Unit unit = sec.PageSetup.PageWidth - sec.PageSetup.LeftMargin - sec.PageSetup.RightMargin;
                    Unit unit2 = Unit.FromCentimeter(15.0);
                    if (unit2 > unit) unit2 = unit;
                    image.Width = unit2;
                    return;
                }

                if (!attributeValue.StartsWith("~/")) return;

                string text2 = HttpContext.Current.Server.MapPath(attributeValue);
                if (File.Exists(text2))
                {
                    Image image2 = paragraph.AddImage(text2);
                    image2.LockAspectRatio = true;
                    Section sec2 = paragraph.Section;
                    Unit unit3 = sec2.PageSetup.PageWidth - sec2.PageSetup.LeftMargin - sec2.PageSetup.RightMargin;
                    Unit unit4 = Unit.FromCentimeter(15.0);
                    if (unit4 > unit3) unit4 = unit3;
                    image2.Width = unit4;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding image " + attributeValue + ": " + ex.Message);
            }
        }

        private void AddHorizontalLine(Section section)
        {
            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.Borders.Bottom.Width = 0.5;
            paragraph.Format.Borders.Bottom.Color = new MigraDoc.DocumentObjectModel.Color(28, 74, 113);
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }

        private void AddInlineParagraphContent(Paragraph paragraph, HtmlNode node, bool parentBold, bool parentItalic, bool parentUnderline, Color? parentColor, int? parentFontSize)
        {
            bool flag = parentBold;
            bool flag2 = parentItalic;
            bool flag3 = parentUnderline;
            Color? parentColor2 = parentColor;
            int? parentFontSize2 = parentFontSize;

            string text = node.Name?.ToLower() ?? string.Empty;
            if (text == "strong" || text == "b") flag = true;
            if (text == "em" || text == "i") flag2 = true;
            if (text == "u") flag3 = true;

            string attributeValue = node.GetAttributeValue("style", "");
            if (!string.IsNullOrEmpty(attributeValue))
            {
                string text2 = ExtractStyleValue(attributeValue, "color");
                if (!string.IsNullOrEmpty(text2)) parentColor2 = ParseColor(text2);

                string text3 = ExtractStyleValue(attributeValue, "font-size");
                if (!string.IsNullOrEmpty(text3))
                {
                    int num = ParseFontSize(text3);
                    if (num > 0) parentFontSize2 = num;
                }

                string text4 = ExtractStyleValue(attributeValue, "font-weight");
                if (!string.IsNullOrEmpty(text4))
                {
                    int result;
                    if (text4.Equals("bold", StringComparison.OrdinalIgnoreCase) || text4.Equals("bolder", StringComparison.OrdinalIgnoreCase))
                        flag = true;
                    else if (int.TryParse(text4, out result) && result >= 600)
                        flag = true;
                }

                string text5 = ExtractStyleValue(attributeValue, "text-decoration");
                if (!string.IsNullOrEmpty(text5) && text5.IndexOf("underline", StringComparison.OrdinalIgnoreCase) >= 0)
                    flag3 = true;

                string text6 = ExtractStyleValue(attributeValue, "font-style");
                if (!string.IsNullOrEmpty(text6) && text6.Equals("italic", StringComparison.OrdinalIgnoreCase))
                    flag2 = true;
            }

            foreach (HtmlNode item in node.ChildNodes)
            {
                if (item.NodeType == HtmlNodeType.Text)
                {
                    string innerText = item.InnerText;
                    if (!string.IsNullOrWhiteSpace(innerText))
                    {
                        FormattedText formattedText = paragraph.AddFormattedText(WebUtility.HtmlDecode(innerText));
                        if (flag) formattedText.Bold = true;
                        if (flag2) formattedText.Italic = true;
                        if (flag3) formattedText.Underline = Underline.Single;
                        if (parentColor2.HasValue) formattedText.Font.Color = parentColor2.Value;
                        if (parentFontSize2.HasValue && parentFontSize2.Value > 0) formattedText.Font.Size = parentFontSize2.Value;
                    }
                }
                else if (item.Name.Equals("br", StringComparison.OrdinalIgnoreCase))
                {
                    paragraph.AddLineBreak();
                }
                else
                {
                    AddInlineParagraphContent(paragraph, item, flag, flag2, flag3, parentColor2, parentFontSize2);
                }
            }
        }

        // =========================================================================
        // UTILITY METHODS
        // =========================================================================

        private string CleanUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;
            string text = url.Trim().Trim('`').Trim('"');
            if (text.IndexOf(' ') >= 0)
                text = text.Replace(" ", "%20");
            return text.Trim();
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        private string ExtractStyleValue(string style, string property)
        {
            if (string.IsNullOrEmpty(style)) return string.Empty;
            string pattern = property + "\\s*:\\s*([^;]+)";
            Match match = Regex.Match(style, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
                return match.Groups[1].Value.Trim();
            return string.Empty;
        }

        private Color ParseColor(string colorValue)
        {
            if (string.IsNullOrEmpty(colorValue)) return Colors.Black;

            if (colorValue.StartsWith("rgb("))
            {
                string text = colorValue.Substring(4, colorValue.Length - 5);
                string[] array = text.Split(',');
                if (array.Length >= 3)
                {
                    int val = int.Parse(array[0].Trim());
                    int val2 = int.Parse(array[1].Trim());
                    int val3 = int.Parse(array[2].Trim());
                    return new MigraDoc.DocumentObjectModel.Color((byte)Math.Min(val, 255), (byte)Math.Min(val2, 255), (byte)Math.Min(val3, 255));
                }
            }
            else if (colorValue.StartsWith("#"))
            {
                string text2 = colorValue.Substring(1);
                if (text2.Length == 3)
                {
                    text2 = new string(new char[6] { text2[0], text2[0], text2[1], text2[1], text2[2], text2[2] });
                }
                if (text2.Length == 6)
                {
                    int val4 = Convert.ToInt32(text2.Substring(0, 2), 16);
                    int val5 = Convert.ToInt32(text2.Substring(2, 2), 16);
                    int val6 = Convert.ToInt32(text2.Substring(4, 2), 16);
                    return new MigraDoc.DocumentObjectModel.Color((byte)Math.Min(val4, 255), (byte)Math.Min(val5, 255), (byte)Math.Min(val6, 255));
                }
            }
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
                }
            }
            return Colors.Black;
        }

        private int ParseFontSize(string fontSizeValue)
        {
            if (string.IsNullOrEmpty(fontSizeValue)) return 0;

            if (fontSizeValue.EndsWith("px"))
            {
                int result;
                if (int.TryParse(fontSizeValue.Replace("px", "").Trim(), out result))
                    return result * 3 / 4;
            }
            else if (fontSizeValue.EndsWith("pt"))
            {
                int result2;
                if (int.TryParse(fontSizeValue.Replace("pt", "").Trim(), out result2))
                    return result2;
            }
            else if (fontSizeValue.EndsWith("em"))
            {
                double result3;
                if (double.TryParse(fontSizeValue.Replace("em", "").Trim(), out result3))
                    return (int)(12.0 * result3);
            }
            else
            {
                int result4;
                if (int.TryParse(fontSizeValue.Trim(), out result4))
                    return result4;
            }
            return 0;
        }

        private bool IsBlankRow(HtmlNode rowNode)
        {
            if (rowNode == null) return true;

            HtmlNodeCollection htmlNodeCollection = rowNode.SelectNodes("./td|./th");
            if (htmlNodeCollection == null || htmlNodeCollection.Count == 0) return true;

            foreach (HtmlNode item in htmlNodeCollection)
            {
                HtmlNodeCollection hrNodes = item.SelectNodes(".//hr");
                if (hrNodes != null && hrNodes.Count > 0)
                {
                    HtmlNodeCollection pNodes = item.SelectNodes(".//p");
                    if (pNodes == null) return true;
                    bool flag = true;
                    foreach (HtmlNode item2 in pNodes)
                    {
                        if (!string.IsNullOrWhiteSpace(item2.InnerText) || item2.SelectNodes(".//img") != null)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) return true;
                }

                if (!string.IsNullOrWhiteSpace(item.InnerText)) return false;
                if (item.SelectNodes(".//img") != null || item.SelectNodes(".//table") != null ||
                    item.SelectNodes(".//ul") != null || item.SelectNodes(".//ol") != null)
                    return false;
            }
            return true;
        }

        private double[] CalculateColumnWidths(HtmlNode tableNode, int columnCount)
        {
            double[] array = new double[columnCount];
            int[] array2 = new int[columnCount];
            double num = 16.0;

            HtmlNodeCollection htmlNodeCollection = tableNode.SelectNodes(".//tr");
            if (htmlNodeCollection != null)
            {
                foreach (HtmlNode item in htmlNodeCollection)
                {
                    HtmlNodeCollection htmlNodeCollection2 = item.SelectNodes(".//th|.//td");
                    if (htmlNodeCollection2 != null)
                    {
                        for (int i = 0; i < htmlNodeCollection2.Count && i < columnCount; i++)
                        {
                            string value = htmlNodeCollection2[i].InnerText?.Trim() ?? "";
                            value = WebUtility.HtmlDecode(value);
                            array2[i] = Math.Max(array2[i], value.Length);
                        }
                    }
                }
            }

            int num2 = array2.Sum();
            if (num2 == 0)
            {
                double num3 = num / (double)columnCount;
                for (int j = 0; j < columnCount; j++)
                    array[j] = num3;
            }
            else
            {
                double val = 2.0;
                double val2 = num * 0.6;
                for (int k = 0; k < columnCount; k++)
                {
                    double val3 = (double)array2[k] / (double)num2 * num;
                    array[k] = Math.Max(val, Math.Min(val2, val3));
                }
                double num4 = array.Sum();
                if (num4 > num)
                {
                    double num5 = num / num4;
                    for (int l = 0; l < columnCount; l++)
                        array[l] *= num5;
                }
                else if (num4 < num)
                {
                    double num6 = num - num4;
                    double num7 = num6 / (double)columnCount;
                    for (int m = 0; m < columnCount; m++)
                        array[m] += num7;
                }
            }
            return array;
        }

        // =========================================================================
        // DOCUMENT360 API HELPERS
        // =========================================================================

        private async Task<string> GetArticleByUrl(string articleUrl)
        {
            HttpClient val = new HttpClient();
            val.DefaultRequestHeaders.Add("api_token", API_TOKEN);
            string text = "https://apihub.document360.io/v2/Articles?url=" + HttpUtility.UrlEncode(articleUrl) + "&isPublished=true";
            HttpResponseMessage val2 = await val.GetAsync(text);
            val2.EnsureSuccessStatusCode();
            return await val2.Content.ReadAsStringAsync();
        }

        private async Task<string> GetCategoryArticles(string categoryId)
        {
            HttpClient val = new HttpClient();
            val.DefaultRequestHeaders.Add("api_token", API_TOKEN);
            string text = "https://apihub.document360.io/v2/Categories/" + categoryId;
            HttpResponseMessage val2 = await val.GetAsync(text);
            val2.EnsureSuccessStatusCode();
            return await val2.Content.ReadAsStringAsync();
        }

        private async Task<string> GetArticleDetail(string articleId)
        {
            HttpClient val = new HttpClient();
            val.DefaultRequestHeaders.Add("api_token", API_TOKEN);
            string text = "https://apihub.document360.io/v2/Articles/" + articleId + "/en?isForDisplay=true";
            HttpResponseMessage val2 = await val.GetAsync(text);
            val2.EnsureSuccessStatusCode();
            return await val2.Content.ReadAsStringAsync();
        }

        private List<string> ExtractArticleIds(dynamic categoryData)
        {
            List<string> list = new List<string>();
            if (categoryData.articles != null)
            {
                foreach (dynamic item in categoryData.articles)
                    list.Add((string)item.id);
            }
            if (categoryData.child_categories != null)
            {
                foreach (dynamic item2 in categoryData.child_categories)
                    list.AddRange(ExtractArticleIds(item2));
            }
            return list;
        }

        private void RenderHtmlTable(Section section, HtmlNode tableNode, Color textColor)
        {
            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            int valueOrDefault = (tableNode.SelectSingleNode(".//tr")?.SelectNodes("./th|./td")?.Count).GetValueOrDefault(1);
            for (int i = 0; i < valueOrDefault; i++)
                table.AddColumn(Unit.FromCentimeter(16.0 / (double)valueOrDefault));

            foreach (HtmlNode item in tableNode.SelectNodes(".//tr"))
            {
                Row row = table.AddRow();
                int num = 0;
                foreach (HtmlNode item2 in item.SelectNodes("./th|./td"))
                {
                    Cell cell = row.Cells[num];
                    Paragraph paragraph = cell.AddParagraph();
                    paragraph.Format.Font.Color = textColor;
                    string text = WebUtility.HtmlDecode(item2.InnerText.Trim());
                    if (!string.IsNullOrEmpty(text))
                        paragraph.AddText(text);
                    HtmlNode htmlNode = item2.SelectSingleNode(".//img");
                    if (htmlNode != null)
                    {
                        string text2 = HttpUtility.HtmlDecode(htmlNode.GetAttributeValue("src", ""));
                        if (!string.IsNullOrEmpty(text2))
                            AddImageToParaSection(text2, section);
                    }
                    num++;
                }
            }
        }

        private void ProcessTableCellContent(Cell cell, HtmlNode cellNode)
        {
            foreach (HtmlNode item in cellNode.ChildNodes)
            {
                if (item.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                {
                    string attributeValue = item.GetAttributeValue("src", "");
                    if (string.IsNullOrEmpty(attributeValue)) continue;
                    try
                    {
                        string url = HttpUtility.HtmlDecode(attributeValue);
                        url = CleanUrl(url);

                        // Change #5: Use image cache
                        string localPath = DownloadImageCached(url);
                        if (localPath == null) continue;

                        Paragraph paragraph = cell.AddParagraph();
                        Image image = paragraph.AddImage(localPath);
                        paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);
                        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                        int index = cell.Column.Index;
                        Unit width = cell.Table.Columns[index].Width;
                        Unit unit = width - Unit.FromCentimeter(0.2);

                        string attributeValue2 = item.GetAttributeValue("width", "");
                        string attributeValue3 = item.GetAttributeValue("height", "");
                        bool flag = false;
                        if (!string.IsNullOrEmpty(attributeValue2))
                        {
                            int result;
                            if (attributeValue2.Equals("auto", StringComparison.OrdinalIgnoreCase))
                            {
                                image.LockAspectRatio = true;
                                image.Width = unit;
                                flag = true;
                            }
                            else if (int.TryParse(attributeValue2.Replace("px", ""), out result))
                            {
                                image.Width = Unit.FromPoint((double)result * 0.75);
                                flag = true;
                            }
                        }
                        if (!string.IsNullOrEmpty(attributeValue3))
                        {
                            int result2;
                            if (attributeValue3.Equals("auto", StringComparison.OrdinalIgnoreCase))
                            {
                                image.LockAspectRatio = true;
                            }
                            else if (int.TryParse(attributeValue3.Replace("px", ""), out result2))
                            {
                                image.Height = Unit.FromPoint((double)result2 * 0.75);
                            }
                        }
                        if (!flag)
                        {
                            image.LockAspectRatio = true;
                            image.Width = unit;
                        }
                        if (image.Width > unit)
                        {
                            image.LockAspectRatio = true;
                            image.Width = unit;
                        }
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                    }
                    catch (Exception)
                    {
                        Paragraph paragraph2 = cell.AddParagraph();
                        paragraph2.AddText("[Image could not be loaded - ProcessTableCellContent]");
                    }
                }
                else if (item.Name.Equals("figure", StringComparison.OrdinalIgnoreCase))
                {
                    HtmlNode htmlNode = item.SelectSingleNode(".//img");
                    if (htmlNode != null)
                    {
                        try
                        {
                            Paragraph para = cell.AddParagraph();
                            int index2 = cell.Column.Index;
                            Unit width2 = cell.Table.Columns[index2].Width;
                            Unit value = width2 - Unit.FromCentimeter(0.2);
                            AddImageToParagraph(para, htmlNode, value);
                        }
                        catch (Exception)
                        {
                            Paragraph paragraph3 = cell.AddParagraph();
                            paragraph3.AddText("[Image could not be loaded - Figure]");
                        }
                    }
                    HtmlNode htmlNode2 = item.SelectSingleNode(".//figcaption");
                    if (htmlNode2 != null && !string.IsNullOrWhiteSpace(htmlNode2.InnerText))
                    {
                        Paragraph paragraph4 = cell.AddParagraph();
                        FormattedText formattedText = paragraph4.AddFormattedText(WebUtility.HtmlDecode(htmlNode2.InnerText.Trim()));
                        formattedText.Italic = true;
                        paragraph4.Format.Alignment = ParagraphAlignment.Center;
                    }
                }
                else if (item.Name.Equals("video", StringComparison.OrdinalIgnoreCase))
                {
                    string attributeValue4 = item.GetAttributeValue("src", string.Empty);
                    attributeValue4 = HttpUtility.HtmlDecode(attributeValue4);
                    attributeValue4 = CleanUrl(attributeValue4);
                    if (!string.IsNullOrEmpty(attributeValue4))
                    {
                        Paragraph paragraph5 = cell.AddParagraph();
                        Hyperlink hyperlink = paragraph5.AddHyperlink(attributeValue4, HyperlinkType.Web);
                        FormattedText formattedText2 = hyperlink.AddFormattedText("Video: " + attributeValue4);
                        formattedText2.Color = new MigraDoc.DocumentObjectModel.Color(0, 106, 138);
                        formattedText2.Underline = Underline.Single;
                    }
                }
                else if (item.Name.Equals("strong", StringComparison.OrdinalIgnoreCase))
                {
                    Paragraph paragraph6 = cell.AddParagraph();
                    paragraph6.AddText(WebUtility.HtmlDecode(item.InnerText));
                    paragraph6.Format.Font.Bold = true;
                    paragraph6.Format.SpaceAfter = Unit.FromPoint(6.0);
                    paragraph6.Format.SpaceBefore = Unit.FromPoint(6.0);
                }
                else if (item.Name.Equals("h1", StringComparison.OrdinalIgnoreCase) || item.Name.Equals("h2", StringComparison.OrdinalIgnoreCase) || item.Name.Equals("h3", StringComparison.OrdinalIgnoreCase) || item.Name.Equals("h4", StringComparison.OrdinalIgnoreCase) || item.Name.Equals("h5", StringComparison.OrdinalIgnoreCase) || item.Name.Equals("h6", StringComparison.OrdinalIgnoreCase))
                {
                    Paragraph paragraph7 = cell.AddParagraph();
                    IEnumerable<HtmlNode> enumerable = item.ChildNodes.Where((HtmlNode n) => n.NodeType == HtmlNodeType.Text);
                    foreach (HtmlNode item2 in enumerable)
                    {
                        if (!string.IsNullOrWhiteSpace(item2.InnerText))
                            paragraph7.AddText(WebUtility.HtmlDecode(item2.InnerText.Trim()));
                    }

                    HtmlNodeCollection htmlNodeCollection = item.SelectNodes(".//span");
                    if (htmlNodeCollection != null)
                    {
                        foreach (HtmlNode item3 in htmlNodeCollection)
                        {
                            if (string.IsNullOrWhiteSpace(item3.InnerText)) continue;
                            string attributeValue5 = item3.GetAttributeValue("style", "");
                            string text2 = ExtractStyleValue(attributeValue5, "color");
                            string text3 = ExtractStyleValue(attributeValue5, "font-size");
                            Font font = new Font();
                            if (!string.IsNullOrEmpty(text2))
                                font.Color = ParseColor(text2);
                            if (!string.IsNullOrEmpty(text3))
                            {
                                int num = ParseFontSize(text3);
                                if (num > 0) font.Size = num;
                            }
                            FormattedText formattedText3 = paragraph7.AddFormattedText(WebUtility.HtmlDecode(item3.InnerText.Trim()));
                            if (!string.IsNullOrEmpty(text2))
                                formattedText3.Font.Color = font.Color;
                            if (!string.IsNullOrEmpty(text3) && font.Size > 0)
                                formattedText3.Font.Size = font.Size;
                        }
                    }

                    switch (item.Name.ToLower())
                    {
                        case "h1":
                            paragraph7.Format.Font.Size = 24;
                            paragraph7.Format.Font.Bold = true;
                            paragraph7.Format.Font.Color = new MigraDoc.DocumentObjectModel.Color(28, 74, 113);
                            paragraph7.Format.SpaceBefore = Unit.FromPoint(10.0);
                            paragraph7.Format.SpaceAfter = Unit.FromPoint(14.0);
                            break;
                        case "h2":
                            paragraph7.Format.Font.Size = 18;
                            paragraph7.Format.Font.Bold = true;
                            paragraph7.Format.Font.Color = Colors.Black;
                            paragraph7.Format.SpaceBefore = Unit.FromPoint(10.0);
                            paragraph7.Format.SpaceAfter = Unit.FromPoint(8.0);
                            break;
                        case "h3":
                            paragraph7.Format.Font.Size = 16;
                            paragraph7.Format.Font.Bold = false;
                            paragraph7.Format.Font.Color = Colors.Black;
                            paragraph7.Format.SpaceBefore = Unit.FromPoint(14.0);
                            paragraph7.Format.SpaceAfter = Unit.FromPoint(8.0);
                            break;
                        case "h4":
                            paragraph7.Format.Font.Size = 14;
                            paragraph7.Format.Font.Bold = false;
                            paragraph7.Format.Font.Color = Colors.Black;
                            paragraph7.Format.SpaceBefore = Unit.FromPoint(12.0);
                            paragraph7.Format.SpaceAfter = Unit.FromPoint(8.0);
                            break;
                        default:
                            paragraph7.Format.Font.Size = 12;
                            paragraph7.Format.Font.Bold = false;
                            paragraph7.Format.Font.Color = Colors.Black;
                            paragraph7.Format.SpaceBefore = Unit.FromPoint(6.0);
                            paragraph7.Format.SpaceAfter = Unit.FromPoint(6.0);
                            break;
                    }
                    paragraph7.Format.KeepWithNext = true;
                    paragraph7.Format.KeepTogether = true;

                    HtmlNodeCollection htmlNodeCollection2 = item.SelectNodes(".//ul");
                    if (htmlNodeCollection2 != null)
                    {
                        foreach (HtmlNode item4 in htmlNodeCollection2)
                        {
                            HtmlNodeCollection htmlNodeCollection3 = item4.SelectNodes("./li");
                            if (htmlNodeCollection3 == null) continue;
                            foreach (HtmlNode item5 in htmlNodeCollection3)
                            {
                                Paragraph paragraph8 = cell.AddParagraph();
                                paragraph8.AddText("• ");
                                IEnumerable<HtmlNode> enumerable2 = item5.ChildNodes.Where((HtmlNode n) => n.NodeType == HtmlNodeType.Text);
                                foreach (HtmlNode item6 in enumerable2)
                                {
                                    if (!string.IsNullOrWhiteSpace(item6.InnerText))
                                        paragraph8.AddText(WebUtility.HtmlDecode(item6.InnerText.Trim()));
                                }
                                HtmlNodeCollection htmlNodeCollection4 = item5.SelectNodes("./span");
                                if (htmlNodeCollection4 != null)
                                {
                                    foreach (HtmlNode item7 in htmlNodeCollection4)
                                    {
                                        if (string.IsNullOrWhiteSpace(item7.InnerText)) continue;
                                        string attributeValue6 = item7.GetAttributeValue("style", "");
                                        string text4 = ExtractStyleValue(attributeValue6, "color");
                                        string text5 = ExtractStyleValue(attributeValue6, "font-size");
                                        FormattedText formattedText4 = paragraph8.AddFormattedText(WebUtility.HtmlDecode(item7.InnerText.Trim()));
                                        if (!string.IsNullOrEmpty(text4))
                                            formattedText4.Font.Color = ParseColor(text4);
                                        if (!string.IsNullOrEmpty(text5))
                                        {
                                            int num2 = ParseFontSize(text5);
                                            if (num2 > 0)
                                                formattedText4.Font.Size = num2;
                                        }
                                    }
                                }
                                paragraph8.Format.SpaceBefore = Unit.FromPoint(6.0);
                                paragraph8.Format.SpaceAfter = Unit.FromPoint(6.0);
                            }
                        }
                    }
                }
                else if (item.Name.Equals("ul", StringComparison.OrdinalIgnoreCase))
                {
                    HtmlNodeCollection htmlNodeCollection5 = item.SelectNodes("./li");
                    if (htmlNodeCollection5 == null) continue;
                    foreach (HtmlNode item8 in htmlNodeCollection5)
                    {
                        Paragraph paragraph9 = cell.AddParagraph();
                        paragraph9.Format.LeftIndent = Unit.FromCentimeter(0.2);
                        paragraph9.Format.FirstLineIndent = Unit.FromCentimeter(-0.2);

                        HtmlNodeCollection htmlNodeCollection6 = item8.SelectNodes("./p");
                        if (htmlNodeCollection6 != null && htmlNodeCollection6.Count > 0)
                        {
                            foreach (HtmlNode item9 in htmlNodeCollection6)
                            {
                                if (item9 == htmlNodeCollection6[0])
                                {
                                    paragraph9.AddText("• ");
                                    paragraph9.AddText(WebUtility.HtmlDecode(item9.InnerText.Trim()));
                                    continue;
                                }
                                Paragraph paragraph10 = cell.AddParagraph();
                                paragraph10.Format.LeftIndent = Unit.FromCentimeter(1.0);
                                paragraph10.AddText(WebUtility.HtmlDecode(item9.InnerText.Trim()));
                                paragraph10.Format.SpaceAfter = Unit.FromPoint(3.0);
                            }
                        }
                        else
                        {
                            IEnumerable<HtmlNode> enumerable3 = item8.ChildNodes.Where((HtmlNode n) => n.NodeType == HtmlNodeType.Text);
                            foreach (HtmlNode item10 in enumerable3)
                            {
                                if (!string.IsNullOrWhiteSpace(item10.InnerText))
                                {
                                    paragraph9.AddText("• ");
                                    paragraph9.AddText(WebUtility.HtmlDecode(item10.InnerText.Trim()));
                                }
                            }
                            HtmlNodeCollection htmlNodeCollection7 = item8.SelectNodes(".//span");
                            if (htmlNodeCollection7 != null)
                            {
                                foreach (HtmlNode item11 in htmlNodeCollection7)
                                {
                                    if (string.IsNullOrWhiteSpace(item11.InnerText)) continue;
                                    string attributeValue7 = item11.GetAttributeValue("style", "");
                                    string text6 = ExtractStyleValue(attributeValue7, "color");
                                    string text7 = ExtractStyleValue(attributeValue7, "font-size");
                                    FormattedText formattedText5 = paragraph9.AddFormattedText(WebUtility.HtmlDecode(item11.InnerText.Trim()));
                                    if (!string.IsNullOrEmpty(text6))
                                        formattedText5.Font.Color = ParseColor(text6);
                                    if (!string.IsNullOrEmpty(text7))
                                    {
                                        int num3 = ParseFontSize(text7);
                                        if (num3 > 0)
                                            formattedText5.Font.Size = num3;
                                    }
                                }
                            }
                        }
                        paragraph9.Format.SpaceBefore = Unit.FromPoint(6.0);
                        paragraph9.Format.SpaceAfter = Unit.FromPoint(6.0);
                    }
                }
                else if (item.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                {
                    Paragraph paragraph11 = cell.AddParagraph();
                    paragraph11.Format.KeepTogether = true;
                    HtmlNode htmlNode3 = item.SelectSingleNode(".//img");
                    if (htmlNode3 != null)
                    {
                        string attributeValue8 = htmlNode3.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(attributeValue8))
                        {
                            try
                            {
                                string text8 = HttpUtility.HtmlDecode(attributeValue8);

                                // Change #5: Use image cache
                                string localPath = DownloadImageCached(text8);
                                if (localPath != null)
                                {
                                    Image image2 = paragraph11.AddImage(localPath);
                                    int index3 = cell.Column.Index;
                                    if (index3 >= 0 && index3 < cell.Table.Columns.Count)
                                        image2.Width = Unit.FromCentimeter(cell.Table.Columns[index3].Width.Centimeter - 0.5);
                                    else
                                        image2.Width = Unit.FromCentimeter(5.0);
                                    image2.LockAspectRatio = true;
                                }
                            }
                            catch (Exception ex3)
                            {
                                paragraph11.AddText(ex3.Message);
                            }
                        }
                    }
                    else
                    {
                        AddInlineParagraphContent(paragraph11, item, false, false, false, null, null);
                    }
                    paragraph11.Format.SpaceBefore = Unit.FromPoint(6.0);
                    paragraph11.Format.SpaceAfter = Unit.FromPoint(6.0);
                }
                else if (item.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(item.InnerText))
                {
                    Paragraph paragraph12 = cell.AddParagraph();
                    paragraph12.AddText(WebUtility.HtmlDecode(item.InnerText.Trim()));
                    paragraph12.Format.SpaceAfter = Unit.FromPoint(0.3);
                    paragraph12.Format.SpaceBefore = Unit.FromPoint(0.3);
                }
            }
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
}
