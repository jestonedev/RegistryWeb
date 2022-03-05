using Microsoft.AspNetCore.Http;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using System;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.RegistryObjects
{
    public class BuildingDemolitionInfoVM
    {
        public int IdBuilding { get; set; }
        public DateTime? DemolishedPlanDate { get; set; }
        public DateTime? DemolishedFactDate { get; set; }
        public DateTime? DateOwnerEmergency { get; set; }
        public DateTime? DemandForDemolishingDeliveryDate { get; set; }
        public List<BuildingDemolitionActFile> BuildingDemolitionActFiles { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
