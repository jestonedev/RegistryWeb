using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
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
