using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class DocumentIssuedBy
    {
        public int IdDocumentIssuedBy { get; set; }
        public string DocumentIssuedByName { get; set; }
        public byte Deleted { get; set; }
    }
}
