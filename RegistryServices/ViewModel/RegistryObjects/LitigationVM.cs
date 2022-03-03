using RegistryDb.Models.Entities;
using RegistryWeb.Enums;

namespace RegistryServices.ViewModel.RegistryObjects
{
    public class LitigationVM : Litigation
    {
        public AddressTypes AddressType { get; set; }

        public LitigationVM() { }

        public LitigationVM(Litigation litigation, AddressTypes type)
        {
            IdLitigation = litigation.IdLitigation;
            IdLitigationType = litigation.IdLitigationType;
            Number = litigation.Number;
            Date = litigation.Date;
            Description = litigation.Description;
            FileOriginName = litigation.FileOriginName;
            FileDisplayName = litigation.FileDisplayName;
            FileMimeType = litigation.FileMimeType;
            Deleted = litigation.Deleted;
            AddressType = type;
        }
    }
}
