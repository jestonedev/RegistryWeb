﻿using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class PrivatizationListVM : ListVM<PrivatizationFilter>
    {
        public List<PrivContract> PrivContracts { get; set; }
    }
}