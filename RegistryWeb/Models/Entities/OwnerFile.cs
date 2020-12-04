using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public class OwnerFile
    {
        public OwnerFile()
        {
            OwnerFilesAssoc = new List<OwnerFileAssoc>();
        }
        public int Id { get; set; }
        public int IdProcess { get; set; }
        public int IdReasonType { get; set; }
        public string ReasonNumber { get; set; }
        public DateTime ReasonDate { get; set; }
        public string FileOriginName { get; set; }
        public string FileDisplayName { get; set; }
        public string FileMimeType { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<OwnerFileAssoc> OwnerFilesAssoc { get; set; }
        public virtual OwnerProcess OwnerProcess { get; set; }
        public virtual OwnerReasonType OwnerReasonType { get; set; }
    }
}
