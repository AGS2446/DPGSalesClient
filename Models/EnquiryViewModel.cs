using System;
using System.Collections.Generic;
using System.Linq;

namespace DPGSalesClient.Models
{
    public class EnquiryViewModel
    {
        public string SearchKey { get; set; }
        public string RoleID { get; set; }
        public int Addcount { get; set; }
        public List<EnquiryViewItemModel> EnquiryList { get; set; }
        public class EnquiryViewItemModel
        {

            public string Division { get; set; }
            public string Branch { get; set; }
            public string EnquiryID { get; set; }
            public string SAPEnquiryID { get; set; }
            public string LeadID { get; set; }
            public string CustomerName { get; set; }
            public string ProjectName { get; set; }
            public string BusineeSegment { get; set; }
            public double? ContractValue { get; set; }
            public string Probablity { get; set; }
            public string Status { get; set; }
            public DateTime? CreatedOn { get; set; }
            public bool isDirect { get; set; }

        }
    }
}
