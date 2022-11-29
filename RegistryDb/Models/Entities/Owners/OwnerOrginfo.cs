using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities.Owners
{
    public partial class OwnerOrginfo : IEquatable<OwnerOrginfo>
    {
        public int IdOwner { get; set; }
        [Required(ErrorMessage = "Поле «Наименование» является обязательным для заполнения")]
        public string OrgName { get; set; }

        public virtual Owner IdOwnerNavigation { get; set; }

        public bool Equals(OwnerOrginfo oo)
        {
            if (oo == null)
                return false;
            if (ReferenceEquals(this, oo))
                return true;
            return IdOwner == oo.IdOwner && OrgName == oo.OrgName;
        }
    }
}
