using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{

    public class SalesReportSearchModel
    {
        public string Division { get; set; }
        public string Branch { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }        
        public SelectList DivisionList { get; set; }
        public SelectList BranchList { get; set; }

    }
    public class SalesReportViewModel
    {
        public string Name { get; set; }
        public int OrderCount { get; set; }
        public Nullable<double> OrderValue { get; set; }
        public Nullable<double> CountPercent { get; set; }
        public Nullable<double> OrderPercent { get; set; }
    }

    public class SalesReportExportViewModel
    {
        public string CRMOrderID { get; set; }
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string BusinessSegment { get; set; }
        public string SubSegment { get; set; }
        public string OrderType { get; set; }
        public Nullable<double> TotalValue { get; set; }
        public string UserID { get; set; }
        public string Username { get; set; }
        public Nullable<double> Tonnage { get; set; }

        public virtual List<SalesReportExportItemViewModel> Items { get; set; }

    }


    public class SalesReportExportItemViewModel
    {
        public string ProductName { get; set; }
        public Nullable<int> Quantity { get; set; }
    }


    public class DonutViewModel
    {
        public string label { get; set; }
        public double value { get; set; }
        public string color { get; set; }
        public string highlight { get; set; }
    }

    public class BarViewModel
    {
        public List<string> labelData { get; set; }
        public List<double> c_PrevMonthData { get; set; }
        public List<double> c_CurrentMonthData { get; set; }
        public List<double> c_UptoCurrentMonthData { get; set; }
        public List<double> p_PrevMonthData { get; set; }
        public List<double> p_CurrentMonthData { get; set; }
        public List<double> p_UptoCurrentMonthData { get; set; }
    }
}
