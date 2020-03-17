﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class PremisesListFilter : FilterAddressOptions
    {
        public string Street { get; set; }
        public int? IdPremisesType { get; set; }
        public int? IdObjectState { get; set; }
        public int? IdFundType { get; set; }

        public bool IsEmpty()
        {
            return IsAddressEmpty() &&
                (IdObjectState == null || IdObjectState.Value == 0);
        }
    }
}
