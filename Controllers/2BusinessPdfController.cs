using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using PickupAPi.Models;
using PickupAPi.Services;

namespace PickupAPi.Controllers
{
    [RoutePrefix("api/BusinessPdf")]
    public class BusinessPdfController : ApiController
    {
        private readonly PdfRendererService _pdfService;

        public BusinessPdfController()
        {
            _pdfService = new PdfRendererService(
                new HtmlParserService(
                    new TableService(),
                    new ImageService(),
                    new BlockquoteService()
                )
            );
        }

        [HttpPost]
        [Route("GeneratePdf")]
        [ResponseType(typeof(byte[]))]
        public async Task<HttpResponseMessage> GeneratePdf([FromBody] HtmlRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request?.htmlContent))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid HTML Content");

            try
            {
                byte[] pdfBytes = _pdfService.GenerateBusinessPdf(request.htmlContent);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(pdfBytes)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "NomadixBusinessDocument.pdf"
                    };

                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}