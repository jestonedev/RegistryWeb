using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class AclUserPrivilegeConfiguration : IEntityTypeConfiguration<AclUserPrivilege>
    {
        private string nameDatebase;

        public AclUserPrivilegeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<AclUserPrivilege> builder)
        {
            builder.HasKey(e => new { e.IdUser, e.IdPrivilege });

            builder.ToTable("acl_user_privileges", nameDatebase);

            builder.HasIndex(e => e.IdPrivilegeType)
                .HasName("FK_acl_user_privileges_acl_privilege_type_id_privilege_type");

            builder.HasIndex(e => e.IdPrivilege)
                .HasName("FK_acl_user_privileges_acl_privileges_id_privilege");

            builder.Property(e => e.IdUser)
                .HasColumnName("id_user")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPrivilege)
                .HasColumnName("id_privilege")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPrivilegeType)
                .HasColumnName("id_privilege_type")
                .HasColumnType("int(11)")
                .HasDefaultValue(1);

            builder.HasOne(d => d.IdAclPrivilegeTypeNavigation)
                .WithMany(p => p.AclUserPrivileges)
                .HasForeignKey(d => d.IdPrivilegeType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_acl_user_privileges_acl_privilege_type_id_privilege_type");

            builder.HasOne(d => d.IdAclPrivilegeNavigation)
                .WithMany(p => p.AclUserPrivileges)
                .HasForeignKey(d => d.IdPrivilege)
                .HasConstraintName("FK_acl_user_privileges_acl_privileges_id_privilege");

            builder.HasOne(d => d.IdAclUserNavigation)
                .WithMany(p => p.AclUserPrivileges)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_acl_user_privileges_acl_users_id_user");
        }
    }
}