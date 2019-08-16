﻿using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerReason
    {
        public int IdReason { get; set; }
        public int IdOwner { get; set; }
        [Required(ErrorMessage = "Поле «Числитель» является обязательным для заполнения")]
        [Range(1, 1024, ErrorMessage = "Поле «Числитель» должно лежать между {1} и {2}")]
        public int NumeratorShare { get; set; } = 1;
        [Required(ErrorMessage = "Поле «Знаменатель» является обязательным для заполнения")]
        [Range(1, 1024, ErrorMessage = "Поле «Знаменатель» должно лежать между {1} и {2}")]
        public int DenominatorShare { get; set; } = 1;
        [Required(ErrorMessage = "Поле «Тип основания» является обязательным для заполнения")]
        public int IdReasonType { get; set; }

        [Required(ErrorMessage = "Поле «Номер» является обязательным для заполнения")]
        public string ReasonNumber { get; set; }
        [Required(ErrorMessage = "Поле «Дата» является обязательным для заполнения")]
        public DateTime? ReasonDate { get; set; }
        public byte Deleted { get; set; }

        public virtual Owner IdOwnerNavigation { get; set; }
        public virtual OwnerReasonType IdReasonTypeNavigation { get; set; }
    }
}
