using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Enums
{
    public enum ProcessOwnershipTypeEnum
    {
        [Display(Name = "Частн + Муниц")]
        All,

        [Display(Name = "Частная")]
        Private,

        [Display(Name = "Муниципальная")]
        Municipal
    }
}
