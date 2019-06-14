using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPerson
    {
        public int IdOwnerPersons { get; set; }
        public int IdOwnerProcess { get; set; }
        [Required(ErrorMessage = "Поле «Фамилия» является обязательным для заполнения")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Поле «Имя» является обязательным для заполнения")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле «Отчество» является обязательным для заполнения")]
        public string Patronymic { get; set; }
        [Required(ErrorMessage = "Поле «Числитель» является обязательным для заполнения")]
        [Range(1, 1024, ErrorMessage = "Поле «Числитель» должно лежать между {1} и {2}")]
        public int NumeratorShare { get; set; } = 1;
        [Required(ErrorMessage = "Поле «Знаменатель» является обязательным для заполнения")]
        [Range(1, 1024, ErrorMessage = "Поле «Знаменатель» должно лежать между {1} и {2}")]
        public int DenominatorShare { get; set; } = 1;
        public byte Deleted { get; set; }

        public virtual OwnerProcess IdOwnerProcessNavigation { get; set; }
    }
}
