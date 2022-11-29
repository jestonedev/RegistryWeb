using Microsoft.AspNetCore.Http;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryServices.ViewModel.RegistryObjects
{
    public class BuildingDemolitionActFileVM : BuildingDemolitionActFile
    {
        public BuildingDemolitionActFileVM()
        {
        }

        public BuildingDemolitionActFileVM(BuildingDemolitionActFile buildingDemolitionActFile)
        {
            Id = buildingDemolitionActFile.Id;
            IdBuilding = buildingDemolitionActFile.IdBuilding;
            IdActFile = buildingDemolitionActFile.IdActFile;
            IdActTypeDocument = buildingDemolitionActFile.IdActTypeDocument;
            Number = buildingDemolitionActFile.Number;
            Date = buildingDemolitionActFile.Date;
            Name = buildingDemolitionActFile.Name;
            ActFile = buildingDemolitionActFile.ActFile;
            ActTypeDocument = buildingDemolitionActFile.ActTypeDocument;
            File = null;
        }

        public IFormFile File { get; set; }
    }
}
