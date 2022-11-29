namespace RegistryDb.Models.Entities.Privatization
{
    public partial class PrivContractorWarrantTemplate
    {
        public int IdTemplate { get; set; }
        public string WarrantText { get; set; }
        public int? IdCategory { get; set; }
    }
}
