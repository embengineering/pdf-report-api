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

            resultContext.Result = await GeneratePdf(contextController, model);
        }

        private async Task<FileResult> GeneratePdf(Controller contextController, SampleReport model)
        {
            var baseUrl = $"{contextController.Request.Scheme}://{contextController.Request.Host}{contextController.Request.PathBase}";

            // Create a HTML to PDF converter object with default settings
            var htmlToPdfConverter = new HtmlToPdfConverter();

            htmlToPdfConverter.PdfDocumentOptions.TopMargin = 8;
            htmlToPdfConverter.PdfDocumentOptions.BottomMargin = 8;
            htmlToPdfConverter.PdfDocumentOptions.LeftMargin = 8;
            htmlToPdfConverter.PdfDocumentOptions.RightMargin = 8;

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

            htmlToPdfConverter.PdfDocumentOptions.ShowHeader = true;
            await SetHeader(htmlToPdfConverter, contextController, model.Header, baseUrl); 
            
            htmlToPdfConverter.PdfDocumentOptions.ShowFooter = true;
            await SetFooter(htmlToPdfConverter, contextController, model.Footer, baseUrl);

            // Convert the HTML page given by an URL to a PDF document in a memory buffer
            var htmlBody = await contextController.RenderViewAsync(model);
            var outPdfBuffer = htmlToPdfConverter.ConvertHtml(htmlBody, baseUrl);

            // Send the PDF file to browser
            FileResult fileResult = new FileContentResult(outPdfBuffer, "application/pdf")
            {
                FileDownloadName = GetReportName(model.Header, "pdf"),
            };

            return fileResult;
        }

        private async Task SetHeader(HtmlToPdfConverter htmlToPdfConverter, Controller contextController, ReportHeader header, string baseUrl)
        {
            // Set the header height in points
            htmlToPdfConverter.PdfHeaderOptions.HeaderHeight = 60;

            // Set header background color
            htmlToPdfConverter.PdfHeaderOptions.HeaderBackColor = RgbColor.White;

            // Create a HTML element to be added in header
            var headerHtml = await contextController.RenderViewAsync("_SampleReportHeader", header);
            var headerElement = new HtmlToPdfElement(headerHtml, baseUrl);

            // Set the HTML element to fit the container height
            headerElement.FitHeight = true;

            // Add HTML element to header
            htmlToPdfConverter.PdfHeaderOptions.AddElement(headerElement);

            // Calculate the header width based on PDF page size and margins
            var headerWidth = htmlToPdfConverter.PdfDocumentOptions.PdfPageSize.Height -
                              htmlToPdfConverter.PdfDocumentOptions.LeftMargin -
                              htmlToPdfConverter.PdfDocumentOptions.RightMargin;

            // Calculate header height
            var headerHeight = htmlToPdfConverter.PdfHeaderOptions.HeaderHeight;

            // Create a line element for the bottom of the header
            var headerLine = new LineElement(htmlToPdfConverter.PdfDocumentOptions.LeftMargin, headerHeight - 1, headerWidth, headerHeight - 1);

            // Set line color
            headerLine.ForeColor = RgbColor.Gray;

            // Add line element to the bottom of the header
            htmlToPdfConverter.PdfHeaderOptions.AddElement(headerLine);
        }

        private async Task SetFooter(HtmlToPdfConverter htmlToPdfConverter, Controller contextController, ReportFooter footer, string baseUrl)
        {
            // Set the footer height in points
            htmlToPdfConverter.PdfFooterOptions.FooterHeight = 60;

            // Set footer background color
            htmlToPdfConverter.PdfFooterOptions.FooterBackColor = RgbColor.White;

            // Create a HTML element to be added in footer
            var footerHtml = await contextController.RenderViewAsync("_SampleReportFooter", footer);
            var footerElement = new HtmlToPdfElement(footerHtml, baseUrl);

            // Set the HTML element to fit the container height
            footerElement.FitHeight = true;

            // Add HTML element to footer
            htmlToPdfConverter.PdfFooterOptions.AddElement(footerElement);

            // Add page numbering
            // Create a text element with page numbering place holders &p; and & P;
            var footerText = new TextElement(0, 30, "Page &p; of &P;  ",
                new PdfFont("Consolas, Monospace", 9, false));

            // Align the text at the right of the footer
            footerText.TextAlign = HorizontalTextAlign.Right;

            // Set page numbering text color
            footerText.ForeColor = RgbColor.Black;

            // Embed the text element font in PDF
            footerText.EmbedSysFont = true;

            // Add the text element to footer
            htmlToPdfConverter.PdfFooterOptions.AddElement(footerText);

            // Calculate the footer width based on PDF page size and margins
            var footerWidth = htmlToPdfConverter.PdfDocumentOptions.PdfPageSize.Height -
                              htmlToPdfConverter.PdfDocumentOptions.LeftMargin - htmlToPdfConverter.PdfDocumentOptions.RightMargin;

            // Create a line element for the top of the footer
            var footerLine = new LineElement(htmlToPdfConverter.PdfDocumentOptions.LeftMargin, 0, footerWidth, 0);

            // Set line color
            footerLine.ForeColor = RgbColor.Gray;

            // Add line element to the bottom of the footer
            htmlToPdfConverter.PdfFooterOptions.AddElement(footerLine);
        }

        private string GetReportName(ReportHeader modelHeader, string ext)
        {
            return $"Report_{modelHeader.StartDate::yyyyMMdd}_{modelHeader.EndDate::yyyyMMdd}.{ext}";
        }
    }
}
