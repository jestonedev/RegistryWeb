using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Enums
{
    public enum ComparisonSignEnum
    {
        [Display(Name = "≥")]
        GreaterThanOrEqual = 0,

        [Display(Name = "≤")]
        LessThanOrEqual = 1,

        [Display(Name = "=")]
        Equal = 2,

        [Display(Name = "между")]
        Between = 3
    }
}
