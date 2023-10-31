using System.ComponentModel.DataAnnotations;

namespace RegistryServices.Enums
{
    public enum LoadPersonsSourceEnum
    {
        [Display(Name = "Не подгружать")]
        None = 0,
        [Display(Name = "Подгружать из найма")]
        Tenancy = 1,
        [Display(Name = "Подгружать из предыдущей исковой работы")]
        PrevClaim = 2
    }
}
