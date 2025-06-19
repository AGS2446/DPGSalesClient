using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DPGSalesClient.Models
{
    public class EnquiryProduct
    {
        public string EnquiryID { get; set; }
        [Required]
        public string BusinessSegment { get; set; }
        [Required]
        public string ProductSeg { get; set; }
        public string Division { get; set; }
        public string ProductSegID { get; set; }
        [Required]
        public int? Quantity { get; set; }
        public double? TotalTonnage { get; set; }
        [Required]
        public double? TotalValue { get; set; }
        public string CreatedOn { get; set; }
        public OpportunityProxy.EntityState EntityState { get; set; }
        public SelectList ProductList { get; set; }

    }
}
