using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class OrderReportSearchModel
    {
        public string Division { get; set; }
        public string Branch { get; set; }
        public string WonLostStatus { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public SelectList DivisionList { get; set; }
        public SelectList BranchList { get; set; }

    }
    public class OrderReportChartModel
    {
        public List<string> labelData { get; set; }
        public List<double?> monthData { get; set; }
    }
    public class OrderReportExport
    {
        public string Division { get; set; }
        public string Region { get; set; }
        public string Branch { get; set; }     
        public string CustomerName { get; set; }
        public string ProjectName { get; set; }
        public double? CV { get; set; }
        public string BusinessSeg { get; set; }
        public string CustomerSeg { get; set; }
        public string CustomerSubSeg { get; set; }
        public string CustomerType { get; set; }
        public string Status { get; set; }
        public string ProductRequired { get; set; }       
        public string CRMOrderID { get; set; }
        public string OrderStatus { get; set; }        
        public string DocumentCreatedDate { get; set; }
        public string CreatedOn { get; set; }
        public string ModifiedOn { get; set; }
        public string Username { get; set; }
        public string Architect { get; set; }
        public string Consultant { get; set; }
        public string Description { get; set; }
        public string CompetitorName { get; set; }
        public string Reason { get; set; }
        public double? Price { get; set; }
        public string EnquiryNo { get; set; }
        public string QuoteNo { get; set; }
        public string CustomerClassification { get; set; }
        public string Classification3 { get; set; }

        public string Classification4 { get; set; }

    }
}
