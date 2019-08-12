using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public class AclPrivilegeType
    {
        public AclPrivilegeType()
        {
            AclRolePrivileges = new List<AclRolePrivilege>();
            AclUserPrivileges = new List<AclUserPrivilege>();
        }

        public int IdPrivilegeType { get; set; }
        public string PrivilegeType { get; set; }

        public virtual IList<AclRolePrivilege> AclRolePrivileges { get; set; }
        public virtual IList<AclUserPrivilege> AclUserPrivileges { get; set; }
    }
}
