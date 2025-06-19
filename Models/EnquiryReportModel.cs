using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{

    public class EnquiryApprovalIndexModel
    {
        public string Division { get; set; }
        public string Branch { get; set; }
        public string EnquiryNo { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public List<EnquiryViewReportModel> EnquiryList { get; set; }
        public List<ClientProxy.AGS_DropdownEntity> DivisionList { get; set; }
        public List<OrganizationProxy.AGS_DropdownEntity> BranchList { get; set; }

    }
    
    public class EnquiryViewReportModel
    {
        public string EnquiryID { get; set; }
        public string QuoteID { get; set; }
        public string OrderID { get; set; }
        public string ID { get; set; }
        public string Division { get; set; }
        public string Branch { get; set; }
        public string CustomerName { get; set; }
        public string Segment { get; set; }
        public string SubSegment { get; set; }
        public double? TotalValue { get; set; }
        public string Status { get; set; }
        public string ExpectedFinalQtr { get; set; }
        public string ExpectedFinalYear { get; set; }
        public double? LostPrice { get; set; }
        public string Remarks { get; set; }
        public int Contactplan { get; set; }
        public int CostSheet { get; set; }
        public string FinalStatus { get; set; }
        public string Username { get; set; }
        public string ApprovedByName { get; set; }

    }

    public class EnquiryExportReportModel
    {
        public string Division { get; set; }
        public string Branch { get; set; }
        public string CustomerName { get; set; }
        public string Segment { get; set; }
        public string SubSegment { get; set; }
        public double? TotalValue { get; set; }
        public string Status { get; set; }
        public string ExpectedFinalQtr { get; set; }
        public string ExpectedFinalYear { get; set; }
        public double? LostPrice { get; set; }
        public string Remarks { get; set; }
        public int Contactplan { get; set; }
        public int CostSheet { get; set; }
  

    }
    public class EnquiryConvertQuoteModel
    {
        public string EnquiryID { get; set; }
        public string QuoteMaturityDate { get; set; }
        public string QuoteValidityDate { get; set; }
        public string QuoteDocumentCreatedDate { get; set; }
        public string EnquiryDocumentCreatedDate { get; set; }
        public string OfferDate { get; set; }
        public List<QuoteProduct> QuoteProducts { get; set; }
    }

    public class EnquiryAttachmentHistoryModel
    {
        public string EnquiryID { get; set; }
        public string LeadID { get; set; }
        public List<DownloadFileObject> Attachments { get; set; }

            
    }
}
