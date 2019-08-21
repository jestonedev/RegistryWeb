using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPerson
    {
        public int IdOwner { get; set; }
        [Required(ErrorMessage = "Поле «Фамилия» является обязательным для заполнения")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Поле «Имя» является обязательным для заполнения")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле «Отчество» является обязательным для заполнения")]
        public string Patronymic { get; set; }

        public virtual Owner IdOwnerNavigation { get; set; }
    }
}
