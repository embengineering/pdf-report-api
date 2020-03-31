using System.Threading.Tasks;
using IronPdf;
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
                resultContext.Result = new BadRequestObjectResult($"Unable to cast object result to {nameof(SampleReport)}");
                return;
            }

            var renderer = await GetConfiguredHtmlToPdfRendererAsync(contextController, model);
            var viewHtml = await contextController.RenderViewAsync(model);
            var pdf = await renderer.RenderHtmlAsPdfAsync(viewHtml);
            var fileName = GetReportName(model.Header, "pdf");

            resultContext.Result = contextController.File(pdf.Stream, "application/pdf", fileName);
        }

        private async Task<HtmlToPdf> GetConfiguredHtmlToPdfRendererAsync(Controller contextController, SampleReport model)
        {
            var renderer = new HtmlToPdf
            {
                PrintOptions =
                {
                    PaperOrientation = PdfPrintOptions.PdfPaperOrientation.Landscape,
                    MarginBottom = 8,
                    MarginTop = 8,
                    MarginLeft = 8,
                    MarginRight = 8,
                    Header = await RenderHeaderAsync(contextController, model.Header),
                    Footer = await RenderFooterAsync(contextController, model.Footer),
                    FitToPaperWidth = true,
                    Title = model.Header.ReportTitle
                }
            };

            renderer.PrintOptions.FitToPaperWidth = true;

            return renderer;
        }

        private static string GetReportName(ReportHeader modelHeader, string ext)
        {
            return $"Report_{modelHeader.StartDate::yyyyMMdd}_{modelHeader.EndDate::yyyyMMdd}.{ext}";
        }

        private static async Task<HtmlHeaderFooter> RenderHeaderAsync(Controller contextController, ReportHeader header)
        {
            var html = await contextController.RenderViewAsync("_SampleReportHeader", header);
            return new HtmlHeaderFooter
            {
                Height = 20,
                HtmlFragment = html,
                DrawDividerLine = true,
                Spacing = 5,
            };
        }

        private static async Task<HtmlHeaderFooter> RenderFooterAsync(Controller contextController, ReportFooter footer)
        {
            var pageCount = "{page} of {total-pages}";
            var html = await contextController.RenderViewAsync("_SampleReportFooter", footer);

            return new HtmlHeaderFooter
            {
                Height = 20,
                HtmlFragment = html.Replace("TEMP_PLACE_HOLDER_PAGE_COUNT", pageCount),
                DrawDividerLine = true,
                Spacing = 5,
            };
        }
    }
}
