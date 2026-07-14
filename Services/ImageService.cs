using HtmlAgilityPack;
using MigraDoc.DocumentObjectModel;
using System.IO;
using System.Net;
using System.Web;

namespace PickupAPi.Services
{
    public class ImageService
    {
        public void Add(Section section, HtmlNode node)
        {
            var src = node.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(src)) return;

            var para = section.AddParagraph();
            para.Format.Alignment = ParagraphAlignment.Center;

            try
            {
                string path = ResolveImage(src);
                var img = para.AddImage(path);
                img.Width = Unit.FromCentimeter(15);
                img.LockAspectRatio = true;
            }
            catch
            {
                para.AddText("[Image could not be loaded]");
            }
        }

        private string ResolveImage(string src)
        {
            if (src.StartsWith("~/"))
                return HttpContext.Current.Server.MapPath(src);

            if (src.StartsWith("http"))
            {
                string tmp = Path.Combine(Path.GetTempPath(), Path.GetFileName(src));
                var client = new WebClient();
                client.DownloadFile(src, tmp);
                return tmp;
            }

            return src;
        }
    }
}