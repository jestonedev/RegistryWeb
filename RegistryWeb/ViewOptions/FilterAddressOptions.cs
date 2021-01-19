using RegistryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions
{
    public class FilterAddressOptions : FilterOptions
    {
        public FilterAddressOptions()
        {
            Address = new Address();
        }

        public Address Address { get; set; }

        public bool IsAddressEmpty()
        {
            return Address == null || Address.AddressType == AddressTypes.None ||
                string.IsNullOrWhiteSpace(Address.Text);
        }
    }
}
