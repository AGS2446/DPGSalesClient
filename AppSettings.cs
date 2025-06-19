using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient
{
    public class AppSettings
    {
        public List<KeyValueObject> Data { get; set; }
    }

    public class KeyValueObject
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
