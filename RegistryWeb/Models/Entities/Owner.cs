using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.Models.Entities
{
    public class Owner : IEquatable<Owner>
    {
        public Owner()
        {
            OwnerReasons = new List<OwnerReason>();
        }

        public int IdOwner { get; set; }
        public int IdProcess { get; set; }
        public int IdOwnerType { get; set; }
        public byte Deleted { get; set; }
                
        public virtual OwnerType IdOwnerTypeNavigation { get; set; }
        public virtual OwnerProcess IdOwnerProcessNavigation { get; set; }
        public virtual OwnerOrginfo OwnerOrginfo { get; set; }
        public virtual OwnerPerson OwnerPerson { get; set; }
        public virtual IList<OwnerReason> OwnerReasons { get; set; }

        public bool Equals(Owner o)
        {
            if (o == null)
                return false;
            if (ReferenceEquals(this, o))
                return true;
            var tmp = true;
            if (OwnerOrginfo != null)
                tmp = tmp && OwnerOrginfo.Equals(o.OwnerOrginfo);
            if (OwnerPerson != null)
                tmp = tmp && OwnerPerson.Equals(o.OwnerPerson);
            return IdOwner == o.IdOwner && IdProcess == o.IdProcess &&
                IdOwnerType == o.IdOwnerType && Deleted == o.Deleted &&
                OwnerReasons.SequenceEqual(o.OwnerReasons) && tmp;
        }
    }
}
