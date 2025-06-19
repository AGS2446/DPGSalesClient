using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class PopupViewModel
    {
        public string Dialog { get; set; }
        public string CssClassHeader { get; set; }
        public string Header { get; set; }
        public string Message { get; set; }

        public List<PopupViewModelMessageItem> MessageItems { get; set; }
    }

    public class PopupViewModelMessageItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
