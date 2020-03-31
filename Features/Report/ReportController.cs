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
            if(!request.StartDate.HasValue || !request.EndDate.HasValue) 
                throw new ArgumentException("Start Date and End Date are required");

            var startRange = int.Parse(request.StartDate.Value.ToString("yyyyMMdd"));
            var endRange = int.Parse(request.EndDate.Value.ToString("yyyyMMdd"));
            var rows = Enumerable.Range(startRange, endRange)
                .Select(i => new SampleReport.SampleRow
                {
                    SampleFoo = i,
                    SampleBar = $"Bar {i}",
                    SampleBaz = DateTimeOffset.Now.AddSeconds(-i)
                })
                .Take(10000)
                .ToArray();

            var sampleReport = new SampleReport
            {
                Header = new ReportHeader
                {
                    StartDate = (DateTimeOffset)request.StartDate,
                    EndDate = (DateTimeOffset)request.EndDate
                },
                Rows = rows,
                Footer = new ReportFooter
                {
                    ReportGenerationDateTime = DateTimeOffset.Now,
                    ReportRunByFullName = "John Doe",
                    Version = "v1"
                }
            };

            return await Task.FromResult(sampleReport);
        }
    }
}
