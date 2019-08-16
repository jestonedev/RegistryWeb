using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class Owner
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
    }
}
