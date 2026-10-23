using Newtonsoft.Json;
using PickupAPi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PickupAPi.Controllers
{
    [RoutePrefix("api/Document360")]
    public class Document360Controller : ApiController
    {
        private static string API_TOKEN = "qstNVgrrO9A6w9byiy2c/n4Cza4lkaOLmXX9KXSx6yH4/0FDSBdOeSnP40bamD7jaJegf5sb0azs9GH1aOALH1qxM74IHURyIdvhC2ijW9tmHyc5TuLG5KOYibNfRaQ0mzMIffNJzcFqff5TtUFb8w==";
        private const string API_BASE = "https://apihub.document360.io";

        // =====================================================================
        // MULTI-WORKSPACE CONTENT AGGREGATION (JWT-gated, permission-filtered)
        // =====================================================================

        [HttpPost]
        [Route("GetMultiWorkspaceContent")]
        public async Task<IHttpActionResult> GetMultiWorkspaceContent([FromBody] MultiWorkspaceRequest req)
        {
            Debug.WriteLine("[DOC360-MW] === START ===");

            // ---- 1. Authenticate caller ----
            string token = ExtractBearerToken();
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("[DOC360-MW] 401 missing token");
                return Unauthorized();
            }

            var claims = JwtHelper.Validate(token);
            if (claims == null)
            {
                Debug.WriteLine("[DOC360-MW] 401 invalid/expired token");
                return Unauthorized();
            }

            var allowedWorkspaces = JwtHelper.GetWorkspaces(claims);
            if (allowedWorkspaces.Count == 0)
            {
                Debug.WriteLine("[DOC360-MW] no authorized workspaces");
                return Ok(new MultiWorkspaceResponse
                {
                    Success = false,
                    Message = "You are not authorized to view any workspace content.",
                    Workspaces = new List<WorkspaceContentResult>(),
                    CombinedHtml = "",
                    TotalWorkspaces = 0,
                    SuccessfulFetches = 0
                });
            }

            if (req == null || req.Urls == null || req.Urls.Count == 0)
                return BadRequest("No URLs provided.");

            // ---- 2. Filter URLs by permission, then fetch survivors ----
            var results = new List<WorkspaceContentResult>();

            foreach (string url in req.Urls)
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    results.Add(new WorkspaceContentResult
                    {
                        Url = url ?? "",
                        Status = "invalid_url",
                        Error = "URL is empty"
                    });
                    continue;
                }

                string workspaceKey = ResolveWorkspaceKey(url);
                if (workspaceKey == null)
                {
                    results.Add(new WorkspaceContentResult
                    {
                        Url = url,
                        Status = "unauthorized",
                        Error = "Unknown workspace"
                    });
                    continue;
                }

                if (!allowedWorkspaces.TryGetValue(workspaceKey, out string permission))
                {
                    Debug.WriteLine("[DOC360-MW] DENIED: " + url);
                    results.Add(new WorkspaceContentResult
                    {
                        Url = url,
                        Workspace = workspaceKey,
                        Status = "unauthorized",
                        Error = "Access not granted"
                    });
                    continue;
                }

                try
                {
                    Debug.WriteLine("[DOC360-MW] Fetching: " + url);

                    string articleJson = await GetArticleByUrl(url);
                    dynamic articleData = JsonConvert.DeserializeObject(articleJson);

                    if (articleData?.data == null)
                    {
                        results.Add(new WorkspaceContentResult
                        {
                            Url = url,
                            Workspace = workspaceKey,
                            Permission = permission,
                            Status = "not_found",
                            Error = "Article not found for this URL"
                        });
                        continue;
                    }

                    string articleId = (string)articleData.data.id;
                    string articleTitle = (string)articleData.data.title;

                    string detailJson = await GetArticleDetail(articleId);
                    dynamic detailData = JsonConvert.DeserializeObject(detailJson);

                    if (detailData?.data == null)
                    {
                        results.Add(new WorkspaceContentResult
                        {
                            Url = url,
                            Workspace = workspaceKey,
                            Permission = permission,
                            Title = articleTitle,
                            Status = "error",
                            Error = "Could not fetch article detail"
                        });
                        continue;
                    }

                    string htmlContent = ((string)detailData.data.html_content) ?? "";
                    string detailTitle = ((string)detailData.data.title) ?? articleTitle;

                    results.Add(new WorkspaceContentResult
                    {
                        Url = url,
                        Workspace = workspaceKey,
                        Permission = permission,
                        Title = detailTitle,
                        HtmlContent = htmlContent,
                        Status = "ok"
                    });

                    Debug.WriteLine("[DOC360-MW] OK: " + detailTitle + " (" + htmlContent.Length + " chars, " + permission + ")");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[DOC360-MW] ERROR: " + url + " -> " + ex.Message);
                    results.Add(new WorkspaceContentResult
                    {
                        Url = url,
                        Workspace = workspaceKey,
                        Permission = permission,
                        Status = "error",
                        Error = ex.Message
                    });
                }
            }

            string combinedHtml = CombineHtmlContent(results);

            var response = new MultiWorkspaceResponse
            {
                Success = results.Any(r => r.Status == "ok"),
                Workspaces = results,
                CombinedHtml = combinedHtml,
                TotalWorkspaces = req.Urls.Count,
                SuccessfulFetches = results.Count(r => r.Status == "ok")
            };

            Debug.WriteLine("[DOC360-MW] Done: " + response.SuccessfulFetches + "/" + response.TotalWorkspaces + " OK");
            return Ok(response);
        }

        // =====================================================================
        // AUTH HELPERS
        // =====================================================================

        private string ExtractBearerToken()
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null &&
                string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                return authHeader.Parameter.Trim();
            }
            return null;
        }

        // URL → workspace key. Covers kms.cloud.global/trustedwifi|nomadix prefixes.
        private string ResolveWorkspaceKey(string url)
        {
            try
            {
                var uri = new Uri(url);
                string path = uri.AbsolutePath.ToLowerInvariant();

                if (path.StartsWith("/trustedwifi/") || path == "/trustedwifi")
                    return "trustedwifi";
                if (path.StartsWith("/nomadix/") || path == "/nomadix")
                    return "nomadix";

                // Also match if host segment contains workspace name (e.g. subdomain style)
                string hostAndPath = (uri.Host + path).ToLowerInvariant();
                if (hostAndPath.Contains("trustedwifi"))
                    return "trustedwifi";
                if (hostAndPath.Contains("nomadix"))
                    return "nomadix";

                return null;
            }
            catch
            {
                return null;
            }
        }

        // =====================================================================
        // DOCUMENT360 API HELPERS
        // =====================================================================

        private async Task<string> GetArticleByUrl(string articleUrl)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api_token", API_TOKEN);
                string apiUrl = API_BASE + "/v2/Articles?url=" + HttpUtility.UrlEncode(articleUrl) + "&isPublished=true";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> GetArticleDetail(string articleId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api_token", API_TOKEN);
                string apiUrl = API_BASE + "/v2/Articles/" + articleId + "/en?isForDisplay=true";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private string CombineHtmlContent(List<WorkspaceContentResult> results)
        {
            var sb = new StringBuilder();
            sb.Append("<div class=\"multi-workspace-content\">");

            foreach (var ws in results.Where(r => r.Status == "ok"))
            {
                sb.Append("<div class=\"workspace-section\">");
                sb.Append("<h2 class=\"workspace-title\">" + HttpUtility.HtmlEncode(ws.Title) + "</h2>");
                sb.Append("<div class=\"workspace-meta\">Source: " + HttpUtility.HtmlEncode(ws.Url) +
                          " · Permission: " + HttpUtility.HtmlEncode(ws.Permission) + "</div>");
                sb.Append("<div class=\"workspace-body\">");
                sb.Append(ws.HtmlContent);
                sb.Append("</div>");
                sb.Append("</div>");
            }

            var denied = results.Where(r => r.Status == "unauthorized").ToList();
            if (denied.Any())
            {
                sb.Append("<div class=\"workspace-errors\">");
                sb.Append("<h3>Access Not Granted</h3><ul>");
                foreach (var d in denied)
                    sb.Append("<li>" + HttpUtility.HtmlEncode(d.Url) + "</li>");
                sb.Append("</ul></div>");
            }

            var failures = results.Where(r => r.Status != "ok" && r.Status != "unauthorized").ToList();
            if (failures.Any())
            {
                sb.Append("<div class=\"workspace-errors\">");
                sb.Append("<h3>Unavailable Content</h3><ul>");
                foreach (var f in failures)
                    sb.Append("<li>" + HttpUtility.HtmlEncode(f.Url) + " — " + HttpUtility.HtmlEncode(f.Error) + "</li>");
                sb.Append("</ul></div>");
            }

            sb.Append("</div>");
            return sb.ToString();
        }
    }

    // =====================================================================
    // MODELS
    // =====================================================================

    public class MultiWorkspaceRequest
    {
        public List<string> Urls { get; set; }
    }

    public class WorkspaceContentResult
    {
        public string Url { get; set; }
        public string Workspace { get; set; }
        public string Permission { get; set; }   // read | edit
        public string Title { get; set; }
        public string HtmlContent { get; set; }
        public string Status { get; set; } // ok, not_found, error, invalid_url, unauthorized
        public string Error { get; set; }
    }

    public class MultiWorkspaceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<WorkspaceContentResult> Workspaces { get; set; }
        public string CombinedHtml { get; set; }
        public int TotalWorkspaces { get; set; }
        public int SuccessfulFetches { get; set; }
    }
}
