using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{

    public class QuoteViewModel
    {
        public string SearchKey { get; set; }
        public List<QuoteViewItemModel> QuoteList { get; set; }
        public class QuoteViewItemModel
        {
            public string Division { get; set; }
            public string Branch { get; set; }
            public string QuoteID { get; set; }
            public string EnquiryID { get; set; }
            public string LeadID { get; set; }
            public string CustomerName { get; set; }
            public string BusineeSegment { get; set; }
            public double? ContractValue { get; set; }
            public string Probablity { get; set; }
            public string Status { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
    }
    public class QuoteNewModel
    {
        [Required]
        public string Division { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string Branch { get; set; }
        [Required]
        public string Plant { get; set; }
        [Required]
        public string SalesOffice { get; set; }


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


        [Required]
        public string Status { get; set; }
        [Required]
        public string ProdcutRequired { get; set; }

        [Required]
        public string Probability { get; set; }
        [Required]
        public string ProjectName { get; set; }

        [Required]
        public double? ContractValue_IN_LAKHS { get; set; }   
        public string Currency { get; set; }
        public double? CurrencyValue { get; set; }
        public string QuoteValidityDate { get; set; }
        [Required]
        public string BusinessSegment { get; set; }
   
        public string DocumentCreatedDate { get; set; }
        public string QuoteMaturityDate { get; set; }
        public string QuoteOfferDate { get; set; }

        [Required]
        public string QuoteAssignTo { get; set; }

        [Required]
        public string Classification1 { get; set; }
        public string Classification2 { get; set; }
        [Required]
        public string Classification3 { get; set; }
        public string Classification4 { get; set; }

        public double? Tonnage { get; set; }
        public double? TotalValue { get; set; }
        public string Architect { get; set; }
        public string Consultant { get; set; }

        public string SourceType { get; set; }
        public string EnquiryDescription { get; set; }
        public string OldDescription { get; set; }

        public string QuoteDescription { get; set; }
        public string LeadID { get; set; }
        public string EnquiryID { get; set; }
        public string QuoteID { get; set; }

        public string strProducts { get; set; }
        public List<QuoteProduct> QuoteProducts { get; set; }

        public List<SelectListItemObject> DivisionList { get; set; }
        public List<SelectListItemObject> RegionList { get; set; }
        public List<SelectListItemObject> BranchList { get; set; }
        public List<SelectListItemObject> PlantsList { get; set; }
        public List<SelectListItemObject> CustomerSegmentList { get; set; }
        public List<SelectListItemObject> CustomerSubSegmentList { get; set; }
        public List<SelectListItemObject> CustomerClassificationList { get; set; }
        public List<SelectListItemObject> CustomerTypeList { get; set; }
        public List<SelectListItemObject> BusinessSegmentList { get; set; }
        public List<SelectListItemObject> ProdcutRequiredList { get; set; }
        public List<SelectListItemObject> ProbabilityList { get; set; }
        public List<SelectListItemObject> QuoteAssignToList { get; set; }
        public List<SelectListItemObject> Classification1List { get; set; }
        public List<SelectListItemObject> Classification2List { get; set; }
        public List<SelectListItemObject> Classification3List { get; set; }
        public List<SelectListItemObject> Classification4List { get; set; }
        public List<SelectListItemObject> SourceTypeList { get; set; }
        public List<SelectListItemObject> CurrencyList { get; set; }
    }

    public class QuoteProduct
    {
        public string QuoteID { get; set; }
        [Required]
        public string BusinessSegment { get; set; }
        [Required]
        public string ProductSeg { get; set; }
        public string Division { get; set; }
        public string ProductSegID { get; set; }
        [Required]
        public int? Quantity { get; set; }
        [Required]
        public double? TotalTonnage { get; set; }
        public double? TotalValue { get; set; }
        public string CreatedOn { get; set; }
        public QuotationProxy.EntityState EntityState { get; set; }
        public SelectList ProductList { get; set; }

    }

    public class QuoteDetailsModel
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
        public string Currency { get; set; }
        public string Status { get; set; }
        public string ProdcutRequired { get; set; }
        public string Probability { get; set; }
        public string ProjectName { get; set; }
        public double? ContractValue_IN_LAKHS { get; set; }
        public string QuoteValidityDate { get; set; }
        public string BusinessSegment { get; set; }
        public string DocumentCreatedDate { get; set; }
        public string QuoteMaturityDate { get; set; }
        public string QuoteAssignTo { get; set; }
        public string Classification1 { get; set; }
        public string Classification2 { get; set; }
        public string Classification3 { get; set; }
        public string Classification4 { get; set; }
        public string QuoteOfferDate { get; set; }
        public double? Tonnage { get; set; }
        public double? TotalValue { get; set; }
        public string Architect { get; set; }
        public string Consultant { get; set; }
        public string SourceType { get; set; }
        public string EnquiryDescription { get; set; }
        public string OldDescription { get; set; }
        public string QuoteDescription { get; set; }
        public string LeadID { get; set; }
        public string EnquiryID { get; set; }
        public string QuoteID { get; set; }
        public string strProducts { get; set; }
        public List<EnquiryProduct> QuoteProducts { get; set; }
        public List<DownloadFileObject> Files { get; set; }
    }
    public class QuoteItemModel
    {
        public string QuoteID { get; set; }
        public string BusinessSegment { get; set; }
        public List<QuoteProduct> Products { get; set; }
    }

    public class QuoteConvertOrderModel
    {
        public string QuoteID { get; set; }
        public string LeadID { get; set; }
        public string EnquiryID { get; set; }
        public string PONo { get; set; }
        public string PODate { get; set; }
        public string OrderDocumentCreatedDate { get; set; }
        public string QuoteDocumentCreatedDate { get; set; }
        public string OrderType { get; set; }
        public string WonLossValue { get; set; }
        public string Reasons { get; set; }
        public string CompName { get; set; }
        public double? TotalItemValue { get; set; }
        public double? ContractValue { get; set; }
        public double? TotalCost { get; set; }
        public double? TotalTonnage { get; set; }
        public double? CompititorPrice { get; set; }
        public double? TurnOver { get; set; }
        public double? GrossMargin { get; set; }
        public string BusinessSegment { get; set; }

        public string CompName2 { get; set; }

        public string Division { get; set; }
        public List<OrderProduct> OrderProducts { get; set; }
        public List<SelectListItemObject> OrderTypeList { get; set; }
        public List<SelectListItemObject> WonLossList { get; set; }
        public List<SelectListItemObject> ReasonsList { get; set; }
        public List<SelectListItemObject> CompNameList { get; set; }




    }


    public class QuoteHistoryModel
    {
        public string QuoteID { get; set; }
        public string Status { get; set; }
        public List<QuotationProxy.AGS_QuotationHistory> HistoryRecords { get; set; }
    }
}
