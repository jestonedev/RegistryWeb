using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.ViewModel
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Не указано имя")]
        public string User { get; set; }
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
