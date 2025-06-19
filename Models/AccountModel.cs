using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class AccountModel
    {
        public string SAPAccountID { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string MobileNo { get; set; }
        public DateTime? CreatedOn { get; set; } 

    }

    public class CustomerObject
    {
        public string customerId { get; set; }
        public string sapCustomerID { get; set; }
        public string customerName { get; set; }
        public string city { get; set; }
        public string state { get; set; }              
        public string contactperson { get; set; }
        public string emailid { get; set; }
        public string customeraddress { get; set; }
        public string pincode { get; set; }
        public string mobilenumber { get; set; }
    }
}
