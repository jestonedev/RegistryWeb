using RegistryDb.Models.Entities.RegistryObjects.Common;

namespace RegistryDb.Models.Entities.RegistryObjects.Buildings
{
    public partial class BuildingAttachmentFileAssoc
    {
        public int IdBuilding { get; set; }
        public int IdAttachment { get; set; }
        public byte Deleted { get; set; }

        public virtual Building BuildingNavigation { get; set; }
        public virtual ObjectAttachmentFile ObjectAttachmentFileNavigation { get; set; }
    }
}
