using System;
using System.Collections.Generic;
using System.Linq;

namespace DPGSalesClient.Models
{
    public class EnquiryItemModel
    {
        public string EnquiryID { get; set; }
        public string BusinessSegment { get; set; }        

        public List<EnquiryProduct> Products { get; set; }
    }
}
