using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class DocumentsIssuedBy
    {
        public int IdDocumentIssuedBy { get; set; }
        public string DocumentIssuedBy { get; set; }
        public byte Deleted { get; set; }
    }
}
