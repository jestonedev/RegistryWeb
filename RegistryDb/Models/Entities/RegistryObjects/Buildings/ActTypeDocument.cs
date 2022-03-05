using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Buildings
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
