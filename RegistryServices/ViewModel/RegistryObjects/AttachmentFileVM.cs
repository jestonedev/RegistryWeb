using RegistryDb.Models.Entities;
using RegistryWeb.Enums;

namespace RegistryServices.ViewModel.RegistryObjects
{
    public class AttachmentFileVM : ObjectAttachmentFile
    {
        public AddressTypes AddressType { get; set; }

        public AttachmentFileVM() { }

        public AttachmentFileVM(ObjectAttachmentFile af, AddressTypes type)
        {
            IdAttachment = af.IdAttachment;
            Description = af.Description;
            FileOriginName = af.FileOriginName;
            FileDisplayName = af.FileDisplayName;
            FileMimeType = af.FileMimeType;
            Deleted = af.Deleted;
            BuildingAttachmentFilesAssoc = af.BuildingAttachmentFilesAssoc;
            AddressType = type;
        }
    }
}
