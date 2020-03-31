using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PdfReport.Api.Features.Report
{
    public class SampleReport : IReportResult<SampleReport.SampleRow>
    {
        public IList<SampleRow> Rows { get; set; }
        public ReportHeader Header { get; set; }
        public ReportFooter Footer { get; set; }

        public class SampleRow
        {
            [DisplayName("Sample Foo")]
            public int SampleFoo { get; set; }

            [DisplayName("Sample Bar")]
            public string SampleBar { get; set; }

            [DisplayName("Sample Baz")]
            public DateTimeOffset SampleBaz { get; set; }
        }
    }

    public class ReportHeader
    {
        public string ReportTitle { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string SortedBy { get; set; }
        public string Filters { get; set; }
    }

    public class ReportFooter
    {
        public string Version { get; set; }
        public string SiteName { get; set; }
        public DateTimeOffset ReportGenerationDateTime { get; set; }
        public string ReportRunByFullName { get; set; }
        public string ReportRunByEmployeeNumber { get; set; }
    }

    public interface IReportResult<T>
    {
        IList<T> Rows { get; }
        ReportHeader Header { get; }
        ReportFooter Footer { get; }
    }
}
