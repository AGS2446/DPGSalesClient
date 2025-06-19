using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class EnquiryForeCastReportModel
    {
        public class ReportViewModel
        {
            public string Divisions { get; set; }
           
            public string Branches { get; set; }
            public string Division { get; set; }
            public string Branch { get; set; }
            public List<SelectListItem> DivisionList { get; set; }
            public List<SelectListItem> BranchList { get; set; }
            [Required]
            public string DateFrom { get; set; }
            [Required]
            public string DateTo { get; set; }
        }
        public class LineChartViewModel 
        {
            public List<String> label { get; set; }
            public List<double?> Month { get; set; } 
            public double? value { get; set; }
            public string color { get; set; }
            public string highlight { get; set; }
        }

        public class CrmObjectReportResponse
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string MonthName
            {
                get
                {
                    return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Month);
                }
            }
            public double Total { get; set; }
        }

        public class EnquiryForecastExport
        {
            public string Division { get; set; }
            public string Region { get; set; }
            public string Branch { get; set; }          
            public string CustomerName { get; set; }
            public string ProjectName { get; set; }
            public double? Tonnage { get; set; }
            public double? CV { get; set; }
            public string ProductRequired { get; set; }
            public string CustomerType { get; set; }
            public string Status { get; set; }
            public string Probability { get; set; }        
            public string Consultant { get; set; }
            public string Architect { get; set; }
            public string EnquiryCreatedDate { get; set; }
            public string ModifiedDate { get; set; }
            public string EnquiryMaturityDate { get; set; }
            public string SalesEngg { get; set; }        
        }
    }
}
