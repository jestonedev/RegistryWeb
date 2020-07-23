using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class ResettleInfoToFact
    {
        public int IdKey { get; set; }
        [Required]
        public int IdObject { get; set; }
        [Required]
        public string ObjectType { get; set; }
        [Required]
        public int IdResettleInfo { get; set; }
        public byte Deleted { get; set; }
        public virtual ResettleInfo ResettleInfoNavigation { get; set; }
    }
}
