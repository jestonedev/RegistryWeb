using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class DocumentIssuedBy
    {
        public int IdDocumentIssuedBy { get; set; }
        [Required(ErrorMessage = "Введите название органа")]
        public string DocumentIssuedByName { get; set; }
        public byte Deleted { get; set; }
    }
}
