using DinkToPdf;
using HtmlAgilityPack;
using Microsoft.Ajax.Utilities;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PickupAPi.Controllers
{
    //[EnableCors(origins: "https://nomadix-kms.document360.io", headers: "*", methods: "*")]
    [RoutePrefix("api/BusinessPdf")]
    public class ReportGeneratorController : ApiController
    {
        [HttpPost]
        [Route("generate")]
        public HttpResponseMessage GeneratePdf([FromBody] HtmlPdfRequest model)
        {
            try
            {
                var html = HtmlLayoutBuilder.WrapHtml(model);

                var doc = new HtmlToPdfDocument
                {
                    GlobalSettings = new GlobalSettings
                    {
                        PaperSize = DinkToPdf.PaperKind.A4,
                        Orientation = DinkToPdf.Orientation.Portrait,
                        DocumentTitle = model.Title,
                        DPI = 300
                    }
                };

                // Fix: Use the Add method to populate the read-only Objects collection
                doc.Objects.Add(new ObjectSettings
                {
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8", LoadImages = true },
                    FooterSettings = new FooterSettings
                    {
                        Center = "[page]",
                        Right = model.FooterNote,
                        Line = true,
                        FontSize = 9
                    }
                });

                var converter = new SynchronizedConverter(new PdfTools());
                var pdf = converter.Convert(doc);

                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(pdf)
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "TrustedWiFi_AdministrationGuide.pdf"
                };

                return result;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}