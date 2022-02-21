using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.Entities
{
    public class AclRole
    {
        public AclRole()
        {
            AclRolePrivileges = new List<AclRolePrivilege>();
            AclUserRoles = new List<AclUserRole>();
        }

        public int IdRole { get; set; }
        public string RoleName { get; set; }

        public virtual IList<AclRolePrivilege> AclRolePrivileges { get; set; }
        public virtual IList<AclUserRole> AclUserRoles { get; set; }
    }
}
