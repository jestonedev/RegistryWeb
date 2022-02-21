using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public class TenancyReason
    {
        public int IdReason { get; set; }
        public int IdProcess { get; set; }
        [Required(ErrorMessage = "Выберите тип основания")]
        public int IdReasonType { get; set; }
        public string ReasonNumber { get; set; }
        public DateTime? ReasonDate { get; set; }
        public string ReasonPrepared { get; set; }
        public byte Deleted { get; set; }

        public virtual TenancyProcess IdProcessNavigation { get; set; }

        public virtual TenancyReasonType IdReasonTypeNavigation { get; set; }
    }
}
