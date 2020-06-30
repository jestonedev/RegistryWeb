using Microsoft.AspNetCore.Http;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class BuildingDemolitionInfoVM
    {
        public int IdBuilding { get; set; }
        public DateTime? DemolishedPlanDate { get; set; }
        public List<BuildingDemolitionActFile> BuildingDemolitionActFiles { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
