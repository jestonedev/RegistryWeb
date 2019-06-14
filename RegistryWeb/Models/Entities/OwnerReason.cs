using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerReason
    {
        public int IdReason { get; set; }
        public int IdProcess { get; set; }
        [Required(ErrorMessage = "Поле «Тип основания» является обязательным для заполнения")]
        public int IdReasonType { get; set; }
        [Required(ErrorMessage = "Поле «Номер» является обязательным для заполнения")]
        public string ReasonNumber { get; set; }
        [Required(ErrorMessage = "Поле «Дата» является обязательным для заполнения")]
        public DateTime? ReasonDate { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerReasonType IdReasonTypeNavigation { get; set; }
        public virtual OwnerProcess IdOwnerProcessesNavigation { get; set; }
    }
}
