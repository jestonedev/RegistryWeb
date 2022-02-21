using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class ResettleDocumentType
    {
        public ResettleDocumentType()
        {
            ResettleDocuments = new List<ResettleDocument>();
        }

        public int IdDocumentType { get; set; }
        public string DocumentTypeName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<ResettleDocument> ResettleDocuments { get; set; }
    }
}
