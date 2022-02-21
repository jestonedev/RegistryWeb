using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class AclRolePrivilegeConfiguration : IEntityTypeConfiguration<AclRolePrivilege>
    {
        private string nameDatebase;

        public AclRolePrivilegeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<AclRolePrivilege> builder)
        {
            builder.HasKey(e => new { e.IdRole, e.IdPrivilege });

            builder.ToTable("acl_role_privileges", nameDatebase);

            builder.HasIndex(e => e.IdPrivilegeType)
                .HasName("FK_acl_role_privileges_acl_privilege_type_id_privilege_type");

            builder.HasIndex(e => e.IdPrivilege)
                .HasName("FK_acl_role_privileges_acl_privileges_id_privilege");

            builder.Property(e => e.IdRole)
                .HasColumnName("id_role")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPrivilege)
                .HasColumnName("id_privilege")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPrivilegeType)
                .HasColumnName("id_privilege_type")
                .HasColumnType("int(11)")
                .HasDefaultValue(1);

            builder.HasOne(d => d.IdAclPrivilegeTypeNavigation)
                .WithMany(p => p.AclRolePrivileges)
                .HasForeignKey(d => d.IdPrivilegeType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_acl_role_privileges_acl_privilege_type_id_privilege_type");

            builder.HasOne(d => d.IdAclPrivilegeNavigation)
                .WithMany(p => p.AclRolePrivileges)
                .HasForeignKey(d => d.IdPrivilege)
                .HasConstraintName("FK_acl_role_privileges_acl_privileges_id_privilege");

            builder.HasOne(d => d.IdAclRoleNavigation)
                .WithMany(p => p.AclRolePrivileges)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK_acl_role_privileges_acl_roles_id_role");
        }
    }
}