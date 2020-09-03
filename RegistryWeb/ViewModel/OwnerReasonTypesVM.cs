﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.ViewModel
{
    public class OwnerReasonTypesVM<T>
    {
        public List<OwnerReasonType> ownerReasonTypes { get; set; }
        public OwnerReasonType ownerReasonType { get; set; }
        public PageOptions PageOptions { get; set; }
    }
}
