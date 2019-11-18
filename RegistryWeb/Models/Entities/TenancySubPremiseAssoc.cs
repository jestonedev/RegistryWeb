namespace RegistryWeb.Models.Entities
{
    public class TenancySubPremiseAssoc
    {
        public int IdAssoc { get; set; }
        public int IdSubPremise { get; set; }
        public int IdProcess { get; set; }
        public double? RentTotalArea { get; set; }
        public byte Deleted { get; set; }

        public virtual TenancyProcess IdProcessNavigation { get; set; }
        public virtual SubPremise IdSubPremiseNavigation { get; set; }
    }
}