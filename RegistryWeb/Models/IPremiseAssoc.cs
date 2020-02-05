using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models
{
    public interface IPremiseAssoc
    {
        int IdAssoc { get; set; }
        int IdPremise { get; set; }
        int IdProcess { get; set; }
        Premise PremiseNavigation { get; set; }
    }
}
