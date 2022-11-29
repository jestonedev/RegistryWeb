using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Acl
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
