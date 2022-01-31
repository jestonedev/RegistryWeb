using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class KumiPaymentsFilter : FilterAddressOptions
    {

        public KumiPaymentsFilter()
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
