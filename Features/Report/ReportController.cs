using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PdfReport.Api.Features.Report
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : Controller
    {
        [HttpGet]
        [ReportActionFilter]
        public async Task<ActionResult<SampleReport>> SampleReport([FromQuery]SampleReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.EndDate < request.StartDate)
            {
                ModelState.AddModelError(nameof(SampleReportRequest.EndDate), "End Date must be greater than Start Date");
                return BadRequest(ModelState);
            }

            var sampleReport = new SampleReport
            {
                Header = new ReportHeader
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                },
                Rows = await GetSampleReportRows(request),
                Footer = new ReportFooter
                {
                    ReportGenerationDateTime = DateTimeOffset.Now,
                    ReportRunByFullName = "John Doe",
                    Version = "v1"
                }
            };

            return sampleReport;
        }

        private Task<SampleReport.SampleRow[]> GetSampleReportRows(SampleReportRequest request)
        {
            var startRange = int.Parse(request.StartDate.ToString("yyyyMMdd"));
            var endRange = int.Parse(request.EndDate.ToString("yyyyMMdd"));
            var rows = Enumerable.Range(startRange, endRange)
                .Select(i => new SampleReport.SampleRow
                {
                    SampleFoo = i,
                    SampleBar = $"Bar {i}",
                    SampleBaz = DateTimeOffset.Now.AddSeconds(-i)
                })
                .Take(10000)
                .ToArray();
            return Task.FromResult(rows);
        }
    }
}
