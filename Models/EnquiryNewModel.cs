using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DPGSalesClient.Models
{
    public class EnquiryNewModel
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

        public string EnquiryValidityDate { get; set; }
        [Required]
        public string BusinessSegment { get; set; }

        public string DocumentCreatedDate { get; set; }
        public string AssignedToUser { get; set; }
        public string EnquiryMaturityDate { get; set; }
        [Required]
        public string Currency { get; set; }

        public double? CurrencyValue { get; set; }
        [Required]
        public string LeadAssignTo { get; set; }

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

        public string LeadID { get; set; }
        public string EnquiryID { get; set; }

        public string strProducts { get; set; }
        public List<EnquiryProduct> OpportunityProducts { get; set; }

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
        public List<SelectListItemObject> EnquiryAssignToList { get; set; }
        public List<SelectListItemObject> Classification1List { get; set; }
        public List<SelectListItemObject> Classification2List { get; set; }
        public List<SelectListItemObject> Classification3List { get; set; }
        public List<SelectListItemObject> Classification4List { get; set; }
        public List<SelectListItemObject> SourceTypeList { get; set; }
        public List<SelectListItemObject> CurrencyList { get; set; }
    }
}
