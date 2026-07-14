using HtmlAgilityPack;
using MigraDoc.DocumentObjectModel;
using System.IO;
using System.Web;
using PickupAPi.Utils;

namespace PickupAPi.Services
{
    public class BlockquoteService
    {
        public void Add(Section section, HtmlNode node)
        {
            string text = HtmlUtils.Clean(node.InnerText);
            if (text.Contains("Note"))
                AddStyled(section, "Note", text, Colors.DarkBlue, Colors.White, "~/logo/info_icon.png");
            else if (text.Contains("Warning"))
                AddStyled(section, "Warning", text, Colors.DarkOrange, Colors.White, "~/logo/warning_icon.png");
        }

        private void AddStyled(Section section, string title, string content, Color bg, Color fg, string iconPath)
        {
            var table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(1.5));
            table.AddColumn(Unit.FromCentimeter(14));
            var row = table.AddRow();
            row.Shading.Color = bg;

            var iconCell = row.Cells[0];
            string localPath = HttpContext.Current.Server.MapPath(iconPath);
            if (File.Exists(localPath))
            {
                var img = iconCell.AddImage(localPath);
                img.Width = Unit.FromCentimeter(1);
                img.LockAspectRatio = true;
            }

            var textCell = row.Cells[1];
            var para = textCell.AddParagraph();
            para.AddFormattedText($"{title}:", TextFormat.Bold);
            para.AddLineBreak();
            para.AddText(content.Replace(title, "").Trim());
            para.Format.Font.Color = fg;
        }
    }
}