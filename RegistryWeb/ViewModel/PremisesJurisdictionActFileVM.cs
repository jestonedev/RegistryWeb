using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
namespace RegistryWeb.ViewModel
{
    public class PremisesJurisdictionActFileVM: PremisesJurisdictionActFiles
    {
        public Address Address { get; set; }

        public PremisesJurisdictionActFileVM() { }

        public PremisesJurisdictionActFileVM(PremisesJurisdictionActFiles pjaf, Address address)
        {
            IdPremises = pjaf.IdPremises;
            IdActFileTypeDocument = pjaf.IdActFileTypeDocument;
            IdActFile = pjaf.IdActFile;
            Number = pjaf.Number;
            Date = pjaf.Date;
            Deleted = pjaf.Deleted;
            IdActFileTypeDocumentNavigation = pjaf.IdActFileTypeDocumentNavigation;
            
            Address = address;
        }
    }
}
