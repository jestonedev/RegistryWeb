using System.ComponentModel.DataAnnotations;

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
