using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class ObjectAttachmentFile
    {
        public ObjectAttachmentFile()
        {
            BuildingAttachmentFilesAssoc = new List<BuildingAttachmentFileAssoc>();
        }

        public int IdAttachment { get; set; }
        public string Description { get; set; }
        public string FileOriginName { get; set; }
        public string FileDisplayName { get; set; }
        public string FileMimeType { get; set; }
        public byte Deleted { get; set; }
        
        public virtual IList<BuildingAttachmentFileAssoc> BuildingAttachmentFilesAssoc { get; set; }
    }
}
