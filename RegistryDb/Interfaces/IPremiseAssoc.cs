using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Premises;

namespace RegistryDb.Interfaces
{
    public interface IPremiseAssoc
    {
        int IdAssoc { get; set; }
        int IdPremise { get; set; }
        int IdProcess { get; set; }
        Premise PremiseNavigation { get; set; }
    }
}
