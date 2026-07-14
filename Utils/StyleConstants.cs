using MigraDoc.DocumentObjectModel;

namespace PickupAPi.Utils
{
    public static class StyleConstants
    {
        public static void Apply(Document doc)
        {
            var normal = doc.Styles["Normal"];
            normal.Font.Name = "Montserrat";
            normal.Font.Size = 10;

            var h1 = doc.Styles["Heading1"];
            h1.Font.Name = "Arial";
            h1.Font.Size = 24; h1.Font.Bold = true;
            h1.Font.Color = new Color(28, 74, 113);

            var h2 = doc.Styles["Heading2"];
            h2.Font.Name = "Arial";
            h2.Font.Size = 18; h2.Font.Bold = true;
            h2.Font.Color = Colors.Black;

            var h3 = doc.Styles["Heading3"];
            h3.Font.Name = "Arial";
            h3.Font.Size = 16; h3.Font.Bold = false;
            h3.Font.Color = Colors.Black;
        }
    }

    public static class PdfSections
    {
        public static void AddCoverPage(Document doc)
        {
            var section = doc.AddSection();
            section.PageSetup.TopMargin = Unit.FromCentimeter(0);
            section.AddParagraph("Cover Page").Format.Font.Size = 20;
        }

        public static void AddHeader(Section section)
        {
            var header = section.Headers.Primary.AddParagraph("Administration Guide");
            header.Format.Font.Bold = true;
            header.Format.Font.Size = 12;
        }

        public static void AddFooter(Section section)
        {
            var footer = section.Footers.Primary.AddParagraph();
            footer.AddText($"{System.DateTime.Now:MMMM yyyy} – GlobalReach Confidential");
            footer.Format.Font.Size = 9;
            footer.Format.Alignment = ParagraphAlignment.Left;
            footer.AddTab();
            footer.AddPageField();
        }
    }
}