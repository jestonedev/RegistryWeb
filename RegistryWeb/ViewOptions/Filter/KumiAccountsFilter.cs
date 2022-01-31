using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class KumiAccountsFilter : FilterAddressOptions
    {

        public KumiAccountsFilter()
        {
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }

        public bool IsModalEmpty()
        {
            return true;
        }
    }
}
