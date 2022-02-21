using System;

namespace RegistryDb.Models.Entities
{
    public class BuildingDemolitionActFile
    {
        public int Id { get; set; }
        public int IdBuilding { get; set; }
        public int? IdActFile  { get; set; }
        public int IdActTypeDocument  { get; set; }
        public string Number  { get; set; }
        public DateTime? Date  { get; set; }
        public string Name  { get; set; }
        public byte Deleted  { get; set; }

        public virtual ActFile ActFile { get; set; }
        public virtual ActTypeDocument ActTypeDocument { get; set; }
        public virtual Building Building { get; set; }
    }
}