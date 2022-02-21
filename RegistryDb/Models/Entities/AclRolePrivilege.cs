namespace RegistryDb.Models.Entities
{
    public class AclRolePrivilege
    {
        public int IdRole { get; set; }
        public int IdPrivilege { get; set; }
        public int IdPrivilegeType { get; set; }

        public virtual AclPrivilege IdAclPrivilegeNavigation { get; set; }
        public virtual AclPrivilegeType IdAclPrivilegeTypeNavigation { get; set; }
        public virtual AclRole IdAclRoleNavigation { get; set; }
    }
}
