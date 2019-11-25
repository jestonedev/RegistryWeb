namespace RegistryWeb.Models.Entities
{
    public class TenancyPremiseAssoc
    {
        public int IdAssoc { get; set; }
        public int IdPremise { get; set; }
        public int IdProcess { get; set; }
        public double? RentTotalArea { get; set; }
        public double? RentLivingArea { get; set; }
        public byte Deleted { get; set; }

        public virtual Premise PremiseNavigation { get; set; }
        public virtual TenancyProcess ProcessNavigation { get; set; }
    }
}