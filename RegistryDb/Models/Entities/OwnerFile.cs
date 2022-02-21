using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public class OwnerFile
    {
        public int Id { get; set; }
        public int IdProcess { get; set; }
        public DateTime DateDownload { get; set; }
        public string FileOriginName { get; set; }
        public string FileDisplayName { get; set; }
        public string FileMimeType { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerProcess OwnerProcess { get; set; }
    }
}
