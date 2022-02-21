using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class ResettleInfoTo
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
