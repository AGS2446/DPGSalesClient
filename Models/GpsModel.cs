using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class GpsNavigationViewModel
    {

        public DateTime? GpsDate { get; set; }
        public DateTime? Createdon { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public string Address { get; set; }        
    }

    public class MapViewModel
    {

        public string UserID { get; set; }        
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string LogonName { get; set; }
        public string EmailID { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
