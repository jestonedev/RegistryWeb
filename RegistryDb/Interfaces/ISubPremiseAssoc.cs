using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Premises;

namespace RegistryDb.Interfaces
{
    public interface ISubPremiseAssoc
    {
        int IdAssoc { get; set; }
        int IdSubPremise { get; set; }
        int IdProcess { get; set; }
        SubPremise SubPremiseNavigation { get; set; }
    }
}
