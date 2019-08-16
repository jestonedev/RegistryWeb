using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerOrginfo : IOwner
    {
        public int IdOwner { get; set; }
        [Required(ErrorMessage = "Поле «Наименование» является обязательным для заполнения")]
        public string OrgName { get; set; }

        public virtual Owner IdOwnerNavigation { get; set; }
    }
}
