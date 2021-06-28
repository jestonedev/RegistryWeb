using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class PrivatizationFilter: FilterAddressOptions
    {
        public bool IsModalEmpty()
        {
            return false;
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }
    }
}
