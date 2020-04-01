using System.Threading.Tasks;
using EvoPdf.HtmlToPdfClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PdfReport.Api.Infrastructure;

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

            resultContext.Result = ConvertHtmlToPdf(contextController, model, html);
        }

        private FileResult ConvertHtmlToPdf(Controller contextController, SampleReport model, string html)
        {
            // Create a HTML to PDF converter object with default settings
            var htmlToPdfConverter = new HtmlToPdfConverter();

            // Set HTML Viewer width in pixels which is the equivalent in converter of the browser window width
            // htmlToPdfConverter.HtmlViewerWidth = int.Parse(model.HtmlViewerWidth);

            // Set HTML viewer height in pixels to convert the top part of a HTML page 
            // Leave it not set to convert the entire HTML
            // if (collection["htmlViewerHeightTextBox"].Length > 0)
            //     htmlToPdfConverter.HtmlViewerHeight = int.Parse(model.HtmlViewerHeight);

            // Set PDF page size which can be a predefined size like A4 or a custom size in points 
            // Leave it not set to have a default A4 PDF page
            htmlToPdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter;

            // Set PDF page orientation to Portrait or Landscape
            // Leave it not set to have a default Portrait orientation for PDF page
            htmlToPdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Landscape;

            // Set the maximum time in seconds to wait for HTML page to be loaded 
            // Leave it not set for a default 60 seconds maximum wait time
            // htmlToPdfConverter.NavigationTimeout = model.NavigationTimeout;

            // Set an additional delay in seconds to wait for JavaScript or AJAX calls after page load completed
            // Set this property to 0 if you don't need to wait for such async operations to finish
            // if (model.ConversionDelay > 0)
            //     htmlToPdfConverter.ConversionDelay = model.ConversionDelay;

            // Convert the HTML page given by an URL to a PDF document in a memory buffer
            var baseUrl = $"{contextController.Request.Scheme}://{contextController.Request.Host}{contextController.Request.PathBase}";
            var outPdfBuffer = htmlToPdfConverter.ConvertHtml(html, baseUrl);

            // Send the PDF file to browser
            FileResult fileResult = new FileContentResult(outPdfBuffer, "application/pdf")
            {
                FileDownloadName = GetReportName(model.Header, "pdf"),
            };

            return fileResult;
        }

        // private async Task<HtmlToPdf> GetConfiguredHtmlToPdfRendererAsync(Controller contextController, SampleReport model)
        // {
        //     var renderer = new HtmlToPdf
        //     {
        //         PrintOptions =
        //         {
        //             PaperOrientation = PdfPrintOptions.PdfPaperOrientation.Landscape,
        //             MarginBottom = 8,
        //             MarginTop = 8,
        //             MarginLeft = 8,
        //             MarginRight = 8,
        //             Header = await RenderHeaderAsync(contextController, model.Header),
        //             Footer = await RenderFooterAsync(contextController, model.Footer),
        //             FitToPaperWidth = true,
        //             Title = model.Header.ReportTitle
        //         }
        //     };
        //
        //     renderer.PrintOptions.FitToPaperWidth = true;
        //
        //     return renderer;
        // }
        //
        private static string GetReportName(ReportHeader modelHeader, string ext)
        {
            return $"Report_{modelHeader.StartDate::yyyyMMdd}_{modelHeader.EndDate::yyyyMMdd}.{ext}";
        }
        //
        // private static async Task<HtmlHeaderFooter> RenderHeaderAsync(Controller contextController, ReportHeader header)
        // {
        //     var html = await contextController.RenderViewAsync("_SampleReportHeader", header);
        //     return new HtmlHeaderFooter
        //     {
        //         Height = 20,
        //         HtmlFragment = html,
        //         DrawDividerLine = true,
        //         Spacing = 5,
        //     };
        // }
        //
        // private static async Task<HtmlHeaderFooter> RenderFooterAsync(Controller contextController, ReportFooter footer)
        // {
        //     var pageCount = "{page} of {total-pages}";
        //     var html = await contextController.RenderViewAsync("_SampleReportFooter", footer);
        //
        //     return new HtmlHeaderFooter
        //     {
        //         Height = 20,
        //         HtmlFragment = html.Replace("TEMP_PLACE_HOLDER_PAGE_COUNT", pageCount),
        //         DrawDividerLine = true,
        //         Spacing = 5,
        //     };
        // }
    }
}
