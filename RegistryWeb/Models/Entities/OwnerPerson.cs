using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPerson
    {
        public OwnerPerson()
        {
            OwnerReasons = new List<OwnerReason>();
        }

        public int IdPerson { get; set; }
        public int IdProcess { get; set; }
        [Required(ErrorMessage = "Поле «Фамилия» является обязательным для заполнения")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Поле «Имя» является обязательным для заполнения")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле «Отчество» является обязательным для заполнения")]
        public string Patronymic { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerProcess IdOwnerProcessNavigation { get; set; }
        public virtual IList<OwnerReason> OwnerReasons { get; set; }
    }
}
