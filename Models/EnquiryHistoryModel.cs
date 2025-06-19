using System;
using System.Collections.Generic;
using System.Linq;

namespace DPGSalesClient.Models
{
    public class EnquiryHistoryModel
    {
        public string EnquryId { get; set; }
        public string Status { get; set; }
        public List<OpportunityProxy.AGS_OpportunityHistory> HistoryRecords { get; set; }
    }
}
