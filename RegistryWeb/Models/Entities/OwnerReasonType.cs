using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerReasonType
    {
        public OwnerReasonType()
        {
            OwnerReasons = new List<OwnerReason>();
        }

        public int IdReasonType { get; set; }
        [Required(ErrorMessage = "Укажите наименование типа основания собственности")]
        public string ReasonName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<OwnerReason> OwnerReasons { get; set; }
    }
}
