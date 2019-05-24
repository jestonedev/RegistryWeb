using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class DocumentsResidence
    {
        public int IdDocumentResidence { get; set; }
        public string DocumentResidence { get; set; }
        public byte Deleted { get; set; }
    }
}
