using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models
{
    public interface ISubPremiseAssoc
    {
        int IdAssoc { get; set; }
        int IdSubPremise { get; set; }
        int IdProcess { get; set; }
        SubPremise SubPremiseNavigation { get; set; }
    }
}
