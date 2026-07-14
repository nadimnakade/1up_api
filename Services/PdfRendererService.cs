using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PickupAPi.Utils;
using System.IO;

namespace PickupAPi.Services
{
    public class PdfRendererService
    {
        private readonly HtmlParserService _parser;

        public PdfRendererService(HtmlParserService parser) => _parser = parser;

        public byte[] GenerateBusinessPdf(string htmlContent)
        {
            var doc = new Document();
            StyleConstants.Apply(doc);

            PdfSections.AddCoverPage(doc);

            var section = doc.AddSection();
            PdfSections.AddHeader(section);
            PdfSections.AddFooter(section);

            _parser.ProcessHtmlContent(htmlContent, section);

            var renderer = new PdfDocumentRenderer(true) { Document = doc };
            renderer.RenderDocument();

            var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }
    }
}