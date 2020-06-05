using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public class Address
    {
        public AddressTypes AddressType { get; set; }
        public string Id { get; set; }

        public Dictionary<string, string> IdParents { get; set; }

        public string Text { get; set; }
    }
}
