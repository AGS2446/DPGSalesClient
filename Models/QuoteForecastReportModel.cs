using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class QuoteForecastReportModel
    {
        public string Division { get; set; }
        public string Branch { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string DateRange { get; set; }
        public SelectList DivisionList { get; set; }
        public SelectList BranchList { get; set; }
    }
    public class QuoteReportChartModel
    {
        public List<string> labelData { get; set; }
        public List<double?> monthData { get; set; }
    }
    public class QuoteForecastReportExport
    {
        public string Division { get; set; }       
        public string Region { get; set; }
        public string Branch { get; set; }
        public string CustomerName { get; set; }
        public string ProjectName { get; set; }
        public double? Tonnage { get; set; }
        public double? CV { get; set; }          
        public string BusinessSeg { get; set; }
        public string Customerseg { get; set; }
        public string CustomerSubseg { get; set; }
        public string CustomerType { get; set; }
        public string Status { get; set; }
        public string ProductRequired { get; set; }
        public string Probability { get; set; }
        public string QuoteCreatedDate { get; set; }
        public string QuoteMaturityDate { get; set; }
        public string ModifiedDate { get; set; }
        public string QuoteNo { get; set; }
        public string EnquiryNumber { get; set; }
        public string LeadNo { get; set; }
        public string SalesEngg { get; set; }        
        public string Architect { get; set; }
        public string Consultant { get; set; }
        public string SourceType { get; set; }
        public string Remarks { get; set; }

    }

}


