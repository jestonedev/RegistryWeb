using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class AclPrivilege
    {
        public AclPrivilege()
        {
            AclRolePrivileges = new List<AclRolePrivilege>();
            AclUserPrivileges = new List<AclUserPrivilege>();
        }

        public int IdPrivilege { get; set; }
        public string PrivilegeName { get; set; }
        public long PrivilegeMask { get; set; }
        public string PrivilegeDescription { get; set; }

        public virtual IList<AclRolePrivilege> AclRolePrivileges { get; set; }
        public virtual IList<AclUserPrivilege> AclUserPrivileges { get; set; }
    }
}
