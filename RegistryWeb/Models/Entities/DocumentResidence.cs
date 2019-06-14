using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class DocumentResidence
    {
        public int IdDocumentResidence { get; set; }
        public string DocumentResidenceName { get; set; }
        public byte Deleted { get; set; }
    }
}
