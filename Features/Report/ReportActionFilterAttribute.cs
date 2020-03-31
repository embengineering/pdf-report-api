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

            const string contentType = "application/pdf";

            var fileName = GetReportName(model.Header, "pdf");
            var renderer = GetConfiguredHtmlToPdfRenderer(model);
            var viewHtml = await contextController.RenderViewAsync(model);
            var pdf = renderer.RenderHtmlAsPdf(viewHtml);
            var stream = pdf.Stream;

            resultContext.Result = contextController.File(stream, contentType, fileName);
        }

        private HtmlToPdf GetConfiguredHtmlToPdfRenderer(SampleReport model)
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
                    Header = RenderHeader(model.Header),
                    Footer = RenderFooter(model.Footer),
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

        private static HtmlHeaderFooter RenderHeader(ReportHeader header)
        {
            var html = $@"
<table style=""width: 100%; font-family: Consolas, Monospace;"">
    <thead>
        <tr>
            <td style=""vertical-align:top; width:30%;"">
                Sorted By: {header.SortedBy}
            </td>
            <td style=""text-align: center;"">
                <h2 style=""margin: 0;"">{header.ReportTitle}</h2>
            </td>
            <td style=""text-align: right; vertical-align:top; width:30%;"">
                Filters: {header.Filters}
            </td>
        </tr>
        <tr>
            <td>All times are local site times.</td>
            <td style=""text-align: center;"">
                {header.StartDate.LocalDateTime} - {header.EndDate.LocalDateTime}
            </td>
            <td></td>
        </tr>
    </thead>
</table>
";
            return new HtmlHeaderFooter
            {
                Height = 20,
                HtmlFragment = html,
                DrawDividerLine = true,
                Spacing = 5,
            };
        }

        private static HtmlHeaderFooter RenderFooter(ReportFooter footer)
        {
            var pageCount = "{page} of {total-pages}";
            var html = $@"
<table style=""width: 100%;"">
    <thead>
        <tr>
            <td style=""width:30%;"">
                Run On: {footer.ReportGenerationDateTime}
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td>
                By User: {footer.ReportRunByFullName}
            </td>
            <td style=""text-align: center;"">
                {footer.Version}
            </td>
            <td style=""text-align: right;"">TEMP_PLACE_HOLDER_PAGE_COUNT</td>
        </tr>
    </thead>
</table>"
                .Replace("TEMP_PLACE_HOLDER_PAGE_COUNT", pageCount);

            return new HtmlHeaderFooter
            {
                Height = 20,
                HtmlFragment = html,
                DrawDividerLine = true,
                Spacing = 5,
            };
        }
    }
}
