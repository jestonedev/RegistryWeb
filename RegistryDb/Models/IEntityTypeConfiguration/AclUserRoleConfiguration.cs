using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class AclUserRoleConfiguration : IEntityTypeConfiguration<AclUserRole>
    {
        private string nameDatebase;

        public AclUserRoleConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<AclUserRole> builder)
        {
            builder.HasKey(e => new { e.IdUser, e.IdRole });

            builder.ToTable("acl_user_roles", nameDatebase);

            builder.HasIndex(e => e.IdUser)
                .HasName("FK_acl_user_roles_acl_users_id_user");

            builder.HasIndex(e => e.IdRole)
                .HasName("FK_acl_user_roles_acl_roles_id_role");

            builder.Property(e => e.IdUser)
                .HasColumnName("id_user")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdRole)
                .HasColumnName("id_role")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.IdAclUserNavigation)
                .WithMany(p => p.AclUserRoles)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_acl_user_roles_acl_users_id_user");

            builder.HasOne(d => d.IdAclRoleNavigation)
                .WithMany(p => p.AclUserRoles)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK_acl_user_roles_acl_roles_id_role");
        }
    }
}