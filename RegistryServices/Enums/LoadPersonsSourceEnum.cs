using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
