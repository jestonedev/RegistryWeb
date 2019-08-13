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
        public byte Deleted { get; set; }

        public virtual OwnerProcess IdOwnerProcessNavigation { get; set; }
        public virtual ICollection<OwnerReason> OwnerReasons { get; set; }
    }
}
