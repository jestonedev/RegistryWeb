﻿using RegistryDb.Models.Entities.Privatization;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Kladr
{
    public partial class KladrStreet
    {
        public KladrStreet()
        {
            PrivContracts = new List<PrivContract>();
            Buildings = new List<Building>();
        }

        public string IdStreet { get; set; }
        public string StreetName { get; set; }
        public string StreetLong { get; set; }

        public virtual IList<PrivContract> PrivContracts { get; set; }
        public virtual IList<Building> Buildings { get; set; }
    }
}
