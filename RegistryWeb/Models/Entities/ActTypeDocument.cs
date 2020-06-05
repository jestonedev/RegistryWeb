﻿using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public class ActTypeDocument
    {
        public ActTypeDocument()
        {
            BuildingDemolitionActFiles = new List<BuildingDemolitionActFile>();
        }

        public int Id { get; set; }
        public string ActFileType { get; set; }
        public string Name { get; set; }

        public virtual IList<BuildingDemolitionActFile> BuildingDemolitionActFiles { get; set; }
    }
}
