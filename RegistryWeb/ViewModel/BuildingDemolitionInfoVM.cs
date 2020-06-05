using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class BuildingDemolitionInfoVM
    {
        public DateTime? DemolishPlanDate { get; set; }
        public List<BuildingDemolitionActFile> BuildingDemolitionActFiles { get; set; }
    }
}
