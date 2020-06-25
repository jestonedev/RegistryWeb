using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
namespace RegistryWeb.ViewModel
{
    public class PremisesJurisdictionActFileVM //: PremisesJurisdictionActFiles
    {
        public AddressTypes AddressType { get; set; }

        public PremisesJurisdictionActFileVM() { }

        /*public PremisesJurisdictionActFileVM(PremisesJurisdictionActFiles pjaf, AddressTypes type)
        {            
            IdJurisdiction = pjaf.IdJurisdiction;
            IdPremises = pjaf.IdPremises;
            IdActFileTypeDocument = pjaf.IdActFileTypeDocument;
            IdActFile = pjaf.IdActFile;
            Number = pjaf.Number;
            Date = pjaf.Date;
            FileOriginName = pjaf.FileOriginName;
            FileDisplayName = pjaf.FileDisplayName;
            FileMimeType = pjaf.FileMimeType;
            Deleted = pjaf.Deleted;
            IdActFileTypeDocumentNavigation = pjaf.IdActFileTypeDocumentNavigation;
            PremiseNavigation = pjaf.PremiseNavigation;
            AddressType = type;
        }*/
    }
}
