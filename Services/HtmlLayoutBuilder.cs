using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class HtmlLayoutBuilder
{
    public static string WrapHtml(HtmlPdfRequest model)
    {
        string content = model.Html ?? "";

        // Extract TOC entries from h1/h2/h3 and add anchors
        var tocEntries = new List<string>();
        string htmlWithAnchors = Regex.Replace(content, "<h([1-3])>(.*?)</h\\1>", match =>
        {
            string level = match.Groups[1].Value;
            string heading = match.Groups[2].Value;
            string anchor = heading.ToLower().Replace(" ", "-").Replace(".", "-").Trim();
            tocEntries.Add($"<div style='margin-left:{(int.Parse(level) - 1) * 20}px'><a href=\"#{anchor}\">{heading}</a></div>");
            return $"<a name=\"{anchor}\"></a><h{level}>{heading}</h{level}>";
        }, RegexOptions.IgnoreCase);

        string logoBase64 = "data:image/png;base64,PUT_YOUR_LOGO_BASE64_HERE";

        return $@"
<html>
<head>
  <style>
    body {{ font-family: Arial; font-size: 11pt; margin: 2cm; }}
    h1 {{ font-size: 20pt; color: #1C2D42; }}
    h2 {{ font-size: 16pt; color: #1C2D42; }}
    h3 {{ font-size: 14pt; color: #1C2D42; }}
    table {{ width: 100%; border-collapse: collapse; }}
    th, td {{ border: 1px solid #ccc; padding: 6px; }}
    th {{ background-color: #f2f2f2; }}
    a {{ color: #0E5B9D; text-decoration: none; }}
  </style>
</head>
<body>

<!-- Cover Page -->
<div style='text-align:center; margin-top:100px;'>
  <img src='{logoBase64}' style='width: 120px; margin-bottom: 20px;' />
  <h1>{model.Title}</h1>
  <p>Version: {model.Version}</p>
  <p style='margin-top: 100px; color: gray;'>Confidential</p>
</div>
<div style='page-break-after: always;'></div>

<!-- Table of Contents -->
<h2>Table of Contents</h2>
{string.Join("\n", tocEntries)}
<div style='page-break-after: always;'></div>

<!-- Main Content -->
{htmlWithAnchors}

</body>
</html>";
    }
}


public class HtmlPdfRequest
{
    public string Html { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public string FooterNote { get; set; }
}