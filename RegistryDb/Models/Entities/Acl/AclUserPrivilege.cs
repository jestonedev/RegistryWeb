namespace RegistryDb.Models.Entities.Acl
{
    public class AclUserPrivilege
    {
        public int IdUser { get; set; }
        public int IdPrivilege { get; set; }
        public int IdPrivilegeType { get; set; }

        public virtual AclPrivilege IdAclPrivilegeNavigation { get; set; }
        public virtual AclPrivilegeType IdAclPrivilegeTypeNavigation { get; set; }
        public virtual AclUser IdAclUserNavigation { get; set; }
    }
}