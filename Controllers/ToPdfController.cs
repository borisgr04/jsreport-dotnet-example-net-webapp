using iTextSharp.text;
using iTextSharp.text.pdf;
using jsreport.Binary;
using jsreport.Local;
using jsreport.MVC;
using jsreport.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace NetWebApp.Controllers
{
    [RoutePrefix("api/topdf")]
    public class ToPdfController : ApiController
    {
        [Route("multiple")]
        public HttpResponseMessage GetMultiple()
        {
            var rs = new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();
            List<string> mensajes = new List<string> { "1", "2", "3" };

            List<Stream> streamPdfs = new List<Stream>();
            

            foreach (var m in mensajes)
            {
                var report = rs.RenderAsync(new RenderRequest()
                {
                    Template = new Template()
                    {
                        Recipe = Recipe.PhantomPdf,
                        Engine = Engine.Handlebars,
                        Content = "Hello from pdf, {{message}}",
                        Phantom = 
                        new Phantom {
                            Header = "B&A Systems SAS",
                            Footer = "<div style='text-align:center'>Page number {#pageNum}/{#numPages}</div>",
                        }
                    },
                    Data = new
                    {
                        message = $"B&A Migrando a jsreport for .NET!!! {m}"
                    }
                }).Result;
                
                streamPdfs.Add(report.Content); 
            }
            byte[] result=null;
            
            using (MemoryStream ms = new MemoryStream())
            {
                Document pdfDoc = new Document(PageSize.A4);
                PdfCopy pdfUnido = new PdfCopy(pdfDoc, ms);
                pdfDoc.Open();
                streamPdfs.ForEach(item =>
                {
                    var itemPdf = new PdfReader(item);
                    pdfUnido.AddDocument(itemPdf);
                });
                pdfDoc.Close();
                result = ms.ToArray();
                result = ms.ToArray();
            }
            string nombre = "Test Multiple";
            HttpResponseMessage response = DescargarPdf(result, nombre);
            return response;
        }

        
        public HttpResponseMessage Get()
        {
            var rs = new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();

            

            var report = rs.RenderAsync(new RenderRequest()
            {
                Template = new Template()
                {
                    Recipe = Recipe.PhantomPdf,
                    Engine = Engine.Handlebars,
                    Content = "Hello from pdf, {{message}}",
                    Phantom = new Phantom { Header = "<div style='text-align:center'>Page number {#pageNum}/{#numPages}</div>" }
                },
                Data = new
                {
                    message = "B&A Migrando a jsreport for .NET!!!"
                }
            }).Result;



            string nombre = "Test";
            HttpResponseMessage response = DescargarPdf(ReadFully(report.Content), nombre);
            return response;
        }
        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        private static RenderRequest CustomRenderRequest = new RenderRequest()
        {
            Template = new Template()
            {
                Content = "Helo world from {{message}}",
                Engine = Engine.Handlebars,
                Recipe = Recipe.PhantomPdf
            },
            Data = new
            {
                message = "jsreport for .NET!!!"
            }
        };
        protected HttpResponseMessage DescargarPdf(byte[] res, string fileName)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);

            if (res != null)
            {
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(res));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentLength = res.Length;
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                response.Content.Headers.ContentDisposition.FileName = $"{fileName}.pdf";
            }
            return response;
        }
    }
}
