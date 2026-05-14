using System;
using System.Collections.Generic;
using System.Linq;

namespace DPGSalesClient.Models
{
    public class EnquiryDetailsModel
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
        public string ProdcutRequired { get; set; }
        public string Probability { get; set; }
        public string ProjectName { get; set; }
        public double? ContractValue_IN_LAKHS { get; set; }
        public string EnquiryValidityDate { get; set; }
        public string BusinessSegment { get; set; }
        public string DocumentCreatedDate { get; set; }
        public string AssignedToUser { get; set; }
        public string EnquiryMaturityDate { get; set; }
        public string Currency { get; set; }
        public string Classification1 { get; set; }
        public string Classification2 { get; set; }
        public string Classification3 { get; set; }
        public string CustomerDesignation { get; set; }
        public string Classification4 { get; set; }
        public double? Tonnage { get; set; }
        public double? TotalValue { get; set; }
        public string Architect { get; set; }
        public string Consultant { get; set; }
        public string SourceType { get; set; }
        public string EnquiryDescription { get; set; }
        public string LeadID { get; set; }
        public string EnquiryID { get; set; }
        public string SAPEnquiryID { get; set; }
        public string UserRoleID { get; set; }
        public List<DownloadFileObject> Files { get; set; }
        public List<EnquiryProduct> OpportunityProducts { get; set; }
        public bool isDirect { get; set; }
    }
}
