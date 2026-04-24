using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{

    public class OrderViewModel
    {
        public string SearchKey { get; set; }
        public List<OrderViewItemModel> OrderList { get; set; }
        public class OrderViewItemModel
        {
            public string Division { get; set; }
            public string Branch { get; set; }
            public string OrderID { get; set; }
            public string QuoteID { get; set; }
            public string EnquiryID { get; set; }
            public string LeadID { get; set; }
            public string CustomerName { get; set; }
            public string BusineeSegment { get; set; }
            public double? ContractValue { get; set; }
            public string Probablity { get; set; }
            public string Status { get; set; }
            public DateTime? CreatedOn { get; set; }
            public bool isDirect { get; set; }
        }
    }
    public class OrderNewModel
    {
        public string Division { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string Branch { get; set; }
        [Required]
        public string Plant { get; set; }
        [Required]
        public string SalesOffice { get; set; }

        [Required]
        public string CustomerCode { get; set; }
        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string ContactPerson { get; set; }
        [Required]
        public string CustomerAddress { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string Pincode { get; set; }
        [Required]
        public string CustomerSegment { get; set; }
        [Required]
        public string CustomerSubSegment { get; set; }
        [Required]
        public string CustomerClassification { get; set; }
        [Required]
        public string CustomerType { get; set; }


    
        public string Status { get; set; } 
       
        [Required]
        public string ProjectName { get; set; }
        [Required]
        public string PONo { get; set; }
        [Required]
        public string PODate { get; set; }
        [Required]
        public string OrderType { get; set; }
        [Required]
        public string WonLossValue { get; set; }
        public string Reasons { get; set; }
        public string CompName { get; set; }
        [Required]
        public double? TotalItemValue { get; set; }
        public double? TotalCost { get; set; }
        public double? TotalTonnage { get; set; }
        public double? CompititorPrice { get; set; }
        public double? TurnOver { get; set; }
        public double? GrossMargin { get; set; }
        [Required]
        public double? ContractValue { get; set; }
        [Required]
        public string BusinessSegment { get; set; }
    
        public string DocumentCreatedDate { get; set; }   

      
        public string AssignTo { get; set; }

     
        public string Classification1 { get; set; }
        public string Classification2 { get; set; }
   
        public string Classification3 { get; set; }
        public string Classification4 { get; set; }
        
        public string Architect { get; set; }
        public string Consultant { get; set; }
        
        public string LeadID { get; set; }
        public string EnquiryID { get; set; }
        public string QuoteID { get; set; }
        public string OrderID { get; set; }
        public List<OrderProduct> OrderProducts { get; set; }

        public List<SelectListItemObject> DivisionList { get; set; }
        public List<SelectListItemObject> RegionList { get; set; }
        public List<SelectListItemObject> BranchList { get; set; }
        public List<SelectListItemObject> PlantsList { get; set; }
        public List<SelectListItemObject> CustomerSegmentList { get; set; }
        public List<SelectListItemObject> CustomerSubSegmentList { get; set; }
        public List<SelectListItemObject> CustomerClassificationList { get; set; }
        public List<SelectListItemObject> CustomerTypeList { get; set; }

 
        public List<SelectListItemObject> BusinessSegmentList { get; set; }    
        public List<SelectListItemObject> Classification1List { get; set; }
        public List<SelectListItemObject> Classification2List { get; set; }
        public List<SelectListItemObject> Classification3List { get; set; }
        public List<SelectListItemObject> Classification4List { get; set; }
        public List<SelectListItemObject> OrderTypeList { get; set; }
        public List<SelectListItemObject> WonLossValueList { get; set; }
        public List<SelectListItemObject> ReasonsList { get; set; }
        public List<SelectListItemObject> CompNameList { get; set; }
    }

    public class OrderDetailsModel
    {
        public string Division { get; set; }
        public string Region { get; set; }   
        public string Branch { get; set; }
        public string Plant { get; set; }
        public string SalesOffice { get; set; }
        public string CustomerCode { get; set; }  
        public string CustomerName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ContactPerson { get; set; }
        public string CustomerAddress { get; set; }
        public string MobileNumber { get; set; }
        public string Pincode { get; set; }    
        public string CustomerSegment { get; set; }       
        public string CustomerSubSegment { get; set; }        
        public string CustomerClassification { get; set; }    
        public string CustomerType { get; set; }

        public string Status { get; set; }
        public string ProjectName { get; set; }
        public string PONo { get; set; }
        public string PODate { get; set; }
        public string OrderType { get; set; }
        public string WonLossValue { get; set; }
        public string Reasons { get; set; }
        public string CompName { get; set; }
        public double? TotalItemValue { get; set; }
        public double? TotalCost { get; set; }
        public double? TotalTonnage { get; set; }
        public double? CompititorPrice { get; set; }
        public double? TurnOver { get; set; }
        public double? GrossMargin { get; set; }
        public double? ContractValue { get; set; }
        public string BusinessSegment { get; set; }
        public string DocumentCreatedDate { get; set; }
        public string AssignTo { get; set; }
        public string Classification1 { get; set; }
        public string Classification2 { get; set; }
        public string Classification3 { get; set; }
        public string Classification4 { get; set; }
        public string Architect { get; set; }
        public string Consultant { get; set; }
        public string LeadID { get; set; }
        public string EnquiryID { get; set; }
        public string QuoteID { get; set; }
        public string OrderID { get; set; }
        public List<OrderProduct> OrderProducts { get; set; }

        public List<DownloadFileObject> Files { get; set; }
        public bool isDirect { get; set; }
    }
    public class OrderProduct
    {
        public string WonLossValue { get; set; }
        public string OrderID { get; set; }
        [Required]
        public string BusinessSegment { get; set; }
        [Required]
        public string ProductSeg { get; set; }
        public string ProductSegID { get; set; }
        [Required]
        public int? Quantity { get; set; }
        public double? TotalTonnage { get; set; }
        
        public double? TurnoverValue { get; set; }
        [Required]
        public double? ContractValue { get; set; }
        public double? MarginValue { get; set; }
        public string CreatedOn { get; set; }
        public OrderProxy.EntityState EntityState { get; set; }
        public SelectList ProductSegmentList { get; set; }

    }

    public class OrderItemModel
    {
        public string OrderID { get; set; }
        public string QuoteID { get; set; }
        public string BusinessSegment { get; set; }
        public List<OrderProduct> Products { get; set; }
    }
   
  

  


}
