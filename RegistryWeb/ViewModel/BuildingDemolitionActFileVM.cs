using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
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
