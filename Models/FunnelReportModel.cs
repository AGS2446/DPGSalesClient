using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
   

    public class FunnelReportSearchModel
    {
        public string Division { get; set; }
        public string Branch { get; set; }
        public string Segment { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }        
        public SelectList DivisionList { get; set; }
        public SelectList BranchList { get; set; }
        public List<SelectListItemObject> SegmentList { get; set; }

    }
    public class FunnelReportChartModel
    {
        public List<string> labelData { get; set; }
        public List<double?> monthData { get; set; }
    }

 



}
