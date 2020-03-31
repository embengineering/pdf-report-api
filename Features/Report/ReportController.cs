using System;
using System.Collections.Generic;
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
            if(request.StartDate == null || request.EndDate == null) 
                throw new ArgumentException("Start Date and End Date are required");

            var sampleReport = new SampleReport
            {
                Header = new ReportHeader
                {
                    StartDate = (DateTimeOffset)request.StartDate,
                    EndDate = (DateTimeOffset)request.EndDate
                },
                Rows = new List<SampleReport.SampleRow>
                {
                    new SampleReport.SampleRow
                    {
                        SampleFoo = 1,
                        SampleBar = "Bar 1",
                        SampleBaz = DateTimeOffset.Now
                    },
                    new SampleReport.SampleRow
                    {
                        SampleFoo = 2,
                        SampleBar = "Bar 2",
                        SampleBaz = DateTimeOffset.Now
                    },
                    new SampleReport.SampleRow
                    {
                        SampleFoo = 3,
                        SampleBar = "Bar 3",
                        SampleBaz = DateTimeOffset.Now
                    }
                },
                Footer = new ReportFooter
                {
                    ReportGenerationDateTime = DateTimeOffset.Now,
                    ReportRunByFullName = "John Doe"
                }
            };

            return await Task.FromResult(sampleReport);
        }
    }
}
