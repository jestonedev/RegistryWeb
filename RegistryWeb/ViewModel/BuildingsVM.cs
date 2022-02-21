using RegistryDb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
using RegistryDb.Models.SqlViews;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RegistryWeb.ViewModel
{
    public class BuildingsVM : ListVM<BuildingsFilter>
    {
        public List<Building> Buildings { get; set; }
        public Dictionary<int, bool> IsMunicipalDictionary { get; set; }
        public List<BuildingOwnershipRightCurrent> BuildingsOwnershipRightCurrent { get; set; }

        public SelectList RestrictionsList { get; set; }
        public SelectList ObjectStatesList { get; set; }
        public SelectList KladrRegionsList { get; set; }
        public SelectList KladrStreetsList { get; set; }
        public SelectList OwnershipRightTypesList { get; set; }
        public SelectList GovernmentDecreesList { get; set; }
        public SelectList SignersList { get; set; }
    }
}
