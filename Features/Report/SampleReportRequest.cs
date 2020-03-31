using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PdfReport.Api.Features.Report
{
    public class SampleReportRequest
    {
        [Required]
        [DisplayName("Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DisplayName("End Date")]
        public DateTime EndDate { get; set; }
    }
}
