using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class AclUser
    {
        public AclUser()
        {
            AclUserPrivileges = new List<AclUserPrivilege>();
            AclUserRoles = new List<AclUserRole>();
        }

        public int IdUser { get; set; }
        public string UserName { get; set; }
        public string UserDescription { get; set; }

        public virtual IList<AclUserPrivilege> AclUserPrivileges { get; set; }
        public virtual IList<AclUserRole> AclUserRoles { get; set; }
    }
}
