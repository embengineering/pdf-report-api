using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PdfReport.Api.Infrastructure;
using SelectPdf;

namespace PdfReport.Api.Features.Report
{
    public class ReportActionFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next(); // execute the actual action

            var objectResult = resultContext.Result as ObjectResult;
            var model = objectResult?.Value as SampleReport;
            var contextController = ((Controller)resultContext.Controller);

            if (model == null)
            {
                resultContext.Result = new BadRequestObjectResult(objectResult?.Value);
                return;
            }

            var html = await contextController.RenderViewAsync(model);
            var fileName = GetReportName(model.Header, "pdf");
            var pdfBytes = await ConvertHtmlToPdfAsync(contextController, html, model);

            resultContext.Result = contextController.File(pdfBytes, "application/pdf", fileName);
        }

        private async Task<byte[]> ConvertHtmlToPdfAsync(Controller contextController, string htmlBody, SampleReport model)
        {
            var baseUrl = $"{contextController.Request.Scheme}://{contextController.Request.Host}{contextController.Request.PathBase}";
            var converter = new HtmlToPdf();

            // general options
            converter.Options.PdfPageSize = PdfPageSize.Letter;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
            converter.Options.WebPageWidth = 1024;
            converter.Options.WebPageHeight = 0;
            converter.Options.MarginTop = 30;
            converter.Options.MarginBottom = 30;
            converter.Options.MarginLeft = 30;
            converter.Options.MarginRight = 30;
            converter.Options.DisplayHeader = true;
            converter.Options.DisplayFooter = true;

            // header settings
            converter.Header.DisplayOnFirstPage = true;
            converter.Header.DisplayOnOddPages = true;
            converter.Header.DisplayOnEvenPages = true;

            // add header
            var htmlHeader = await contextController.RenderViewAsync("_SampleReportHeader", model.Header);
            var headerSection = new PdfHtmlSection(htmlHeader, baseUrl)
            {
                AutoFitHeight = HtmlToPdfPageFitMode.AutoFit
            };
            converter.Header.Add(headerSection);

            // footer settings
            converter.Footer.DisplayOnFirstPage = true;
            converter.Footer.DisplayOnOddPages = true;
            converter.Footer.DisplayOnEvenPages = true;

            // add footer
            var htmlFooter = await contextController.RenderViewAsync("_SampleReportFooter", model.Footer);
            var footerSection = new PdfHtmlSection(htmlFooter, baseUrl)
            {
                AutoFitHeight = HtmlToPdfPageFitMode.AutoFit
            };
            converter.Footer.Add(footerSection);

            // page numbers can be added using a PdfTextSection object
            var text = new PdfTextSection(0, 10,
                "{page_number} of {total_pages}  ",
                new System.Drawing.Font("Consolas, Monospace", 12))
            {
                HorizontalAlign = PdfTextHorizontalAlign.Right
            };
            converter.Footer.Add(text);

            // set converter options
            converter.Options.PdfPageSize = PdfPageSize.Letter;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;

            var doc = converter.ConvertHtmlString(htmlBody, baseUrl);

            return doc.Save();
        }

        private static string GetReportName(ReportHeader modelHeader, string ext)
        {
            return $"Report_{modelHeader.StartDate::yyyyMMdd}_{modelHeader.EndDate::yyyyMMdd}.{ext}";
        }
    }
}
