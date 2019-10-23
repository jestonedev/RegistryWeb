﻿using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class OwnerProcessesFilter : FilterOptions
    {
        public OwnerProcessesFilter()
        {
            Address = new Address();
        }

        public Address Address { get; set; }
        public int? IdOwnerType { get; set; }
        public int IdProcessType { get; set; } //все/действующие/аннулированые
        public int? IdProcess { get; set; }

        public bool IsEmpty()
        {
            return (Address.AddressType == AddressTypes.None ||
                string.IsNullOrWhiteSpace(Address.Id) ||
                string.IsNullOrWhiteSpace(Address.Text)) &&
                (IdOwnerType == null || IdOwnerType.Value == 0) &&
                (IdProcess == null || IdProcess.Value == 0) &&
                IdProcessType == 0;
        }
    }
}
