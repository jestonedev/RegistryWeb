using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class PrivContractorWarrantTemplate
    {
        public int IdTemplate { get; set; }
        public string WarrantText { get; set; }
        public int? IdCategory { get; set; }
    }
}
