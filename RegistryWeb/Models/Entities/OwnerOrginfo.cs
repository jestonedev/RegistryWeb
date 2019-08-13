using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerOrginfo
    {
        public OwnerOrginfo()
        {
            OwnerReasons = new List<OwnerReason>();
        }

        public int IdOrginfo { get; set; }
        public int IdProcess { get; set; }
        [Required(ErrorMessage = "Поле «Наименование» является обязательным для заполнения")]
        public string OrgName { get; set; }
        [Required(ErrorMessage = "Поле «Числитель» является обязательным для заполнения")]
        [Range(1, 1024, ErrorMessage = "Поле «Числитель» должно лежать между {1} и {2}")]
        public int NumeratorShare { get; set; } = 1;
        [Required(ErrorMessage = "Поле «Знаменатель» является обязательным для заполнения")]
        [Range(1, 1024, ErrorMessage = "Поле «Знаменатель» должно лежать между {1} и {2}")]
        public int DenominatorShare { get; set; } = 1;
        public byte Deleted { get; set; }

        public virtual OwnerProcess IdOwnerProcessNavigation { get; set; }
        public virtual ICollection<OwnerReason> OwnerReasons { get; set; }
    }
}
