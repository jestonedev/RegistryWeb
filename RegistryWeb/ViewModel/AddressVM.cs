﻿using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class AddressVM
    {
        public IEnumerable<KladrStreets> KladrStreets { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
    }
}
