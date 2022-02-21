﻿using RegistryWeb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class Address
    {
        public AddressTypes AddressType { get; set; }
        public string Id { get; set; }

        public Dictionary<string, string> IdParents { get; set; }

        public string Text { get; set; }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0 ^ AddressType.GetHashCode() ^ IdParents?.GetHashCode() ?? 0 ^ Text?.GetHashCode() ?? 0;
        }
    }
}
