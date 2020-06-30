namespace RegistryWeb.Models.Entities
{
    public class ActFile
    {
        public int IdFile { get; set; }
        public string OriginalName { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }

        public virtual BuildingDemolitionActFile BuildingDemolitionActFile { get; set; }
    }
}
