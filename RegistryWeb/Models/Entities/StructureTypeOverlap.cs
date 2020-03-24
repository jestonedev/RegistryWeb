﻿using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class StructureTypeOverlap
    {
        public StructureTypeOverlap()
        {
            Buildings = new List<Building>();
        }

        public int IdStructureTypeOverlap { get; set; }
        public string StructureTypeOverlapName { get; set; }

        public virtual IList<Building> Buildings { get; set; }
    }
}
