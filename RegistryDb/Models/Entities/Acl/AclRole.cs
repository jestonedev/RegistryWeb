using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Acl
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
