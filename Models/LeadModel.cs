using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{

    public class LeadViewModel
    {
        public string SearchKey { get; set; }
        public List<LeadViewItemModel> LeadList { get; set; }
        public class LeadViewItemModel
        {
            public string Division { get; set; }
            public string Branch { get; set; }
            public string LeadID { get; set; }
            public string CustomerName { get; set; }
            public string ProjectName { get; set; }
            public string BusineeSegment { get; set; }
            public string Status { get; set; }
            public double? ContractValue { get; set; }
            public string Probablity { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
    }


    public class LeadNewModel
    {
        public string LeadID { get; set; }

        [Required]
        public string Division { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string Branch { get; set; }
        [Required]
        public string SalesOffice { get; set; }
        [Required]
        public string Plant { get; set; }
   
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
        public string BusinessSegment { get; set; }
        [Required]
        public string ProdcutRequired { get; set; }
        [Required]
        public string Probability { get; set; }
        [Required]
        public string ProjectName { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public double? ContractValue_IN_LAKHS { get; set; }
        [Required]
        public string Priority { get; set; }
        [Required]
        public string Currency { get; set; }
        public double? CurrencyValue { get; set; }
        [Required]
        public string LeadAssignTo { get; set; }
        [Required]
        public string LeadMaturityDate { get; set; }
        [Required]
        public string Classification1 { get; set; }
        public string Classification2 { get; set; }
        [Required]
        public string Classification3 { get; set; }
        public string Classification4 { get; set; }
        public string Architect { get; set; }
        public string Consultant { get; set; }

        public string DocumentCreatedDate { get; set; }
        public string SourceType { get; set; }
        [Required]
        public string Description { get; set; }

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
        public List<SelectListItemObject> PriorityList { get; set; }
        public List<SelectListItemObject> CurrencyList { get; set; }
        public List<SelectListItemObject> LeadAssignToList { get; set; }
        public List<SelectListItemObject> Classification1List { get; set; }
        public List<SelectListItemObject> Classification2List { get; set; }
        public List<SelectListItemObject> Classification3List { get; set; }
        public List<SelectListItemObject> Classification4List { get; set; }
        public List<SelectListItemObject> SourceTypeList { get; set; }

    }

    public class LeadDetailsModel
    {
        public string LeadID { get; set; }
        public string Division { get; set; }
        public string Region { get; set; }
        public string Branch { get; set; }
        public string SalesOffice { get; set; }
        public string Plant { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
        public string ContactPerson { get; set; }
        public string CustomerAddress { get; set; }
        public string MobileNumber { get; set; }
        public string Pincode { get; set; }
        public string CustomerSegment { get; set; }
        public string CustomerSubSegment { get; set; }
        public string CustomerClassification { get; set; }
        public string CustomerType { get; set; }
        public string BusinessSegment { get; set; }
        public string ProdcutRequired { get; set; }
        public string Probability { get; set; }
        public string ProjectName { get; set; }
        public double? ContractValue_IN_LAKHS { get; set; }
        public string Priority { get; set; }
        public string Currency { get; set; }
        public string LeadAssignTo { get; set; }
        public DateTime? LeadMaturityDate { get; set; }
        public string Classification1 { get; set; }
        public string Classification2 { get; set; }
        public string Classification3 { get; set; }
        public string Classification4 { get; set; }
        public string Architect { get; set; }
        public string Consultant { get; set; }
        public DateTime? DocumentCreatedDate { get; set; }
        public string SourceType { get; set; }
        public string Description { get; set; }

        public List<DownloadFileObject> Files { get; set; }

    }

    public class LeadHistoryModel
    {
        public string LeadId { get; set; }
        public string Status { get; set; }
        public List<LeadProxy.AGS_LeadHistory> HistoryRecords { get; set; }
    }

    public class LeadConvertEnquiryModel
    {
        public string LeadId { get; set; }
        public string BusinessSegmentId { get; set; }
        [Required]
        public string BusinessSegment { get; set; }
        [Required]
        public string EnquiryMaturityDate { get; set; }
        [Required]
        public string EnquiryDocumentCreatedDate { get; set; }
        public string LeadDocumentCreatedDate { get; set; }
        [Required]
        public string EnquiryValidDate { get; set; }
        public double? Tonnage { get; set; }
        public double? TotalValue { get; set; }
        public string Division { get; set; }
        public List<EnquiryProduct> EnquiryProducts { get; set; }

        
    }


    public class PlantSaleOfficeDetails
    {
        public string salesoffice { get; set; }
        public string salesofficeName { get; set; }

        public List<plantInfo> Plants { get; set; }

        public class plantInfo
        {
            public string code { get; set; }
            public string name { get; set; }
        }
    }

    public class DropdownObject
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
