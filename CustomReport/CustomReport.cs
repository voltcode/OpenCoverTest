using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Reporting;

namespace CustomReport
{
    class CustomReport : IReportBuilder
    {
        public string ReportType => throw new NotImplementedException();

        public IReportConfiguration ReportConfiguration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
            throw new NotImplementedException();
        }

        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            throw new NotImplementedException();
        }
    }
}
