using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class BuildingsFilter : FilterAddressOptions
    {
        public int? IdObjectState { get; set; }

        public bool IsEmpty()
        {
            return IsAddressEmpty() &&
                (IdObjectState == null || IdObjectState.Value == 0);
        }
    }
}
