﻿using RegistryDb.Models.Entities.Common;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Privatization
{
    public partial class PrivEstateOwner
    {
        public PrivEstateOwner()
        {
            SelectableSigners = new List<SelectableSigner>();
        }

        public int IdOwner { get; set; }
        public string Name { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<SelectableSigner> SelectableSigners { get; set; }
    }
}
