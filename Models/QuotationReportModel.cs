using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{    
    public class QuotationReportSearchModel
    {
        public string Division { get; set; }
        public string Branch { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public SelectList DivisionList { get; set; }
        public SelectList BranchList { get; set; }

    }
    public class QuotationReportChartModel
    {
        public List<string> labelData { get; set; }
        public List<double?> monthData { get; set; }
    }

    public class QuotationReportExport
    {
        public string Division { get; set; }
        public string Region { get; set; }
        public string Branch { get; set; }
        public string Priority { get; set; }
        public string Plant { get; set; }
        public string SalesOffice { get; set; }
        public string CustomerName { get; set; }
        public string ProjectName { get; set; }
        public double? CV { get; set; }
        public string BusinessSeg { get; set; }
        public string CustomerSeg { get; set; }
        public string CustomerSubSeg { get; set; }
        public string CustomerType { get; set; }
        public string Status { get; set; }
        public string ProductRequired { get; set; }
        public string Currency { get; set; }
        public string Classification1 { get; set; }
        public string Classification2 { get; set; }
        public string Classification3 { get; set; }
        public string Classification4 { get; set; }
        public string Probability { get; set; }
        public string CustomerClassification { get; set; }
        public string CRMQuotationID { get; set; }
        public string DocumentCreatedDate { get; set; }       
        public string QuoteMaturityDate { get; set; }
        public string QuoteValidityDate { get; set; }

        [DisplayName( "QuoteCancelled/Budgetary/Defferred Date" )]
        public string OfferDate { get; set; }
        public string ModifiedOn { get; set; }
        public string Username { get; set; }
        public string Architect { get; set; }
        public string Consultant { get; set; }
        public string Description { get; set; }
        public string ApprovedBy { get; set; }
        public double? Tonnage { get; set; }
        public string EnquiryNo { get; set; }
        public string Remarks { get; set; }


    }
}
