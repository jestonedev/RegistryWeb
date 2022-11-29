using RegistryDb.Models.Entities.RegistryObjects.Premises;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Resettle
{
    public partial class ResettlePremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdResettleInfo { get; set; }
        public byte Deleted { get; set; }

        public virtual ResettleInfo ResettleInfoNavigation { get; set; }
        public virtual Premise PremisesNavigation { get; set; }
    }
}
