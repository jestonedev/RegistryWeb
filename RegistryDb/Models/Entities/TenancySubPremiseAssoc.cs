using RegistryDb.Interfaces;

namespace RegistryDb.Models.Entities
{
    public class TenancySubPremiseAssoc : ISubPremiseAssoc
    {
        public int IdAssoc { get; set; }
        public int IdSubPremise { get; set; }
        public int IdProcess { get; set; }
        public double? RentTotalArea { get; set; }
        public byte Deleted { get; set; }

        public virtual TenancyProcess ProcessNavigation { get; set; }
        public virtual SubPremise SubPremiseNavigation { get; set; }
    }
}