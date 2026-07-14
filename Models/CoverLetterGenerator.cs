using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel;
using System.Web;
using System.IO;


public class CoverPageGenerator
{
    private readonly CoverPageConfig _config;
    private readonly Document _doc;

    public CoverPageGenerator(Document doc, CoverPageConfig config)
    {
        _doc = doc;
        _config = config;
    }

    public void AddCoverPage()
    {
        var section = _doc.AddSection();
        SetupPage(section);
        AddBackgroundImage(section);
        AddConfidentialLabel(section);
        AddTitleContent(section);
        AddFooterContent(section);
    }

    private void SetupPage(Section section)
    {
        section.PageSetup = new PageSetup
        {
            PageWidth = Unit.FromCentimeter(21),
            PageHeight = Unit.FromCentimeter(29.7),
            TopMargin = Unit.FromCentimeter(0),
            BottomMargin = Unit.FromCentimeter(0),
            LeftMargin = Unit.FromCentimeter(0),
            RightMargin = Unit.FromCentimeter(0)
        };
    }

    private void AddBackgroundImage(Section section)
    {
        string backgroundUrl = MapPath(_config.BackgroundImagePath);
        if (File.Exists(backgroundUrl))
        {
            var backgroundImage = section.AddImage(backgroundUrl);
            backgroundImage.Width = section.PageSetup.PageWidth;
            backgroundImage.Height = Unit.FromCentimeter(15);
            backgroundImage.RelativeVertical = RelativeVertical.Page;
            backgroundImage.RelativeHorizontal = RelativeHorizontal.Page;
            backgroundImage.WrapFormat.Style = WrapStyle.None;
        }
    }

    private void AddConfidentialLabel(Section section)
    {
        var confidentialLabel = section.AddParagraph("CONFIDENTIAL");
        ApplyFormat(confidentialLabel, new ParagraphFormatting
        {
            FontSize = 10,
            IsBold = true,
            Color = Colors.White,
            Alignment = ParagraphAlignment.Right,
            SpaceBefore = Unit.FromCentimeter(1),
            ShadingColor = Colors.Green,
            LeftIndent = Unit.FromCentimeter(-1)
        });
    }

    private void AddTitleContent(Section section)
    {
        // Title
        var title = section.AddParagraph(_config.Title);
        ApplyFormat(title, new ParagraphFormatting
        {
            FontSize = 28,
            IsBold = true,
            Color = Colors.White,
            Alignment = ParagraphAlignment.Center,
            SpaceBefore = Unit.FromCentimeter(5)
        });

        // Subtitle
        var subtitle = section.AddParagraph(_config.Subtitle);
        ApplyFormat(subtitle, new ParagraphFormatting
        {
            FontSize = 16,
            Color = Colors.White,
            Alignment = ParagraphAlignment.Center,
            SpaceBefore = Unit.FromCentimeter(1)
        });

        // Version and Author
        var versionAndAuthor = section.AddParagraph($"Ver: {_config.Version}\nBy {_config.Authors}");
        ApplyFormat(versionAndAuthor, new ParagraphFormatting
        {
            FontSize = 12,
            Color = Colors.White,
            Alignment = ParagraphAlignment.Center,
            SpaceBefore = Unit.FromCentimeter(1)
        });
    }

    private void AddFooterContent(Section section)
    {
        // Footer text
        var footer = section.AddParagraph(_config.FooterText);
        ApplyFormat(footer, new ParagraphFormatting
        {
            FontSize = 10,
            IsBold = true,
            Color = Colors.White,
            Alignment = ParagraphAlignment.Center,
            ShadingColor = Colors.Green,
            SpaceBefore = Unit.FromCentimeter(12),
            EnableShading = true
        });

        // Logo
        string logoPath = MapPath(_config.LogoPath);
        if (File.Exists(logoPath))
        {
            var logo = section.AddImage(logoPath);
            logo.Width = Unit.FromCentimeter(4);
            logo.Height = Unit.FromCentimeter(1.2);
            logo.RelativeVertical = RelativeVertical.Page;
            logo.RelativeHorizontal = RelativeHorizontal.Page;
            logo.Left = ShapePosition.Center;
            logo.Top = Unit.FromCentimeter(27.5);
        }
    }

    private class ParagraphFormatting
    {
        public int FontSize { get; set; }
        public bool IsBold { get; set; }
        public Color Color { get; set; }
        public ParagraphAlignment Alignment { get; set; }
        public Unit SpaceBefore { get; set; }
        public Color ShadingColor { get; set; }
        public Unit LeftIndent { get; set; }
        public bool EnableShading { get; set; }
    }

    private void ApplyFormat(Paragraph paragraph, ParagraphFormatting format)
    {
        paragraph.Format.Font.Size = format.FontSize;
        paragraph.Format.Font.Bold = format.IsBold;
        paragraph.Format.Font.Color = format.Color;
        paragraph.Format.Font.Name = "Arial";
        paragraph.Format.Alignment = format.Alignment;
        paragraph.Format.SpaceBefore = format.SpaceBefore;

        if (format.EnableShading)
        {
            paragraph.Format.Shading.Color = format.ShadingColor;
            paragraph.Format.Shading.Visible = true;
        }

        if (format.LeftIndent != Unit.Zero)
        {
            paragraph.Format.LeftIndent = format.LeftIndent;
        }

        paragraph.Format.Borders.Width = 0;
        paragraph.Format.Borders.Color = Colors.Transparent;
    }

    private string MapPath(string path)
    {
        return HttpContext.Current.Server.MapPath(path);
    }
}

public class CoverPageConfig
{
    public string Title { get; set; } = "Nomadix Nexus";
    public string Subtitle { get; set; } = "Project Overview, Architecture Concept & Security Capabilities";
    public string Version { get; set; } = "1.0";
    public string Authors { get; set; } = "Christophe Ameline & Jason Micallef";
    public string FooterText { get; set; } = "For internal use only. Do not distribute.";
    public string BackgroundImagePath { get; set; } = "~/logo/cover.jpg";
    public string LogoPath { get; set; } = "~/logo/nomadix_logo.png";
}



