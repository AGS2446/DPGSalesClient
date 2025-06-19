using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class SalesOrderViewModel
    {       
        public string Division { get; set; }
        public string Branch { get; set; }
        public string OrderID { get; set; }
        public string QuoteID { get; set; }
        public string EnquiryID { get; set; }
        public string LeadID { get; set; }
        public string CustomerName { get; set; }        
        public double? TotalValue { get; set; }        
        public string Status { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class SAPOrdersViewModel
    {
        public string CRMSalesOrderID { get; set; }
        public string SAPOrderNO { get; set; }
        public string OrderID { get; set; }       
        public string OrderType { get; set; }
        public string Customer { get; set; }
        public string SAPRemarks { get; set; }
        public DateTime? CreatedOn { get; set; }

    }

    public class SalesOrderNewModel
    {
        public string CRMOrderID { get; set; }
        public string SalesOrderID { get; set; }
        public string OrderType { get; set; }
        public string SalesOrg { get; set; }
        public string DistChannel { get; set; }
        public string Division { get; set; }
        public string PONumber { get; set; }
        public string PODate { get; set; }
        public string CustomerCode{ get; set; }
        public string CustomerName { get; set; }
        public string ShipToPartyCode { get; set; }
        public string ShipToPartyName { get; set; }
        public string ConditionType { get; set; }
        public string MaterialNo { get; set; }
        public string MaterialDesc { get; set; }
        public string Plant { get; set; }
        public int? Quantity { get; set; }        
        public double? TotalValue { get; set; }
        public string CreatedOn { get; set; }     
        
        public string strItems { get; set; }
        public List<SalesOrderItem> salesOrderItems { get; set; }
        public List<SelectListItemObject> OrderTypeList { get; set; }
        public List<SelectListItemObject> ConditionTypeList { get; set; }
        public List<SelectListItemObject> SalesOrgList { get; set; }
        public List<SelectListItemObject> DistChannelList { get; set; }
        public List<SelectListItemObject> DivisionList { get; set; }                

    }

    public class SalesOrderItem
    {
        public string SalesOrderID { get; set; }    
        public int ItemNO { get; set; }    
        public string MaterialNo { get; set; }
        public string MaterialDesc { get; set; }        
        public string Plant { get; set; }                
        public int? Quantity { get; set; }        
        public double? TotalValue { get; set; }
        public string CreatedOn { get; set; }        
        public SelectList MaterialList { get; set; }
        public List<SelectListItemObject> PlantList { get; set; }
        public PurchaseOrderProxy.EntityState EntityState { get; set; }

    }

    public class SalesOrderItemModel
    {

        public string CRMOrderID { get; set; }
        public string SalesOrderID { get; set; }
        public string Material { get; set; }
        public List<SalesOrderItem> salesOrderItems { get; set; }
    }

    public class SalesOrderDetailsModel
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
    }
}
