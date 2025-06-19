using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
   public class HomeListItem
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        public string Action { get; set; }
        public string Controller { get; set; }
    }
}
