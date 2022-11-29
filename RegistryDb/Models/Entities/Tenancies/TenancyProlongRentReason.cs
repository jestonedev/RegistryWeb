namespace RegistryDb.Models.Entities.Tenancies
{
    public partial class TenancyProlongRentReason
    {
        public TenancyProlongRentReason()
        {
        }

        public int IdReasonType { get; set; }
        public string ReasonName { get; set; }
        public string ReasonTemplateGenetive { get; set; }
        public int Deleted { get; set; }
    }
}
