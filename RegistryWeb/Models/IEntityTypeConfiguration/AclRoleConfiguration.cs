using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class AclRoleConfiguration : IEntityTypeConfiguration<AclRole>
    {
        private string nameDatebase;

        public AclRoleConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<AclRole> builder)
        {
            builder.HasKey(e => e.IdRole);

            builder.ToTable("acl_roles", nameDatebase);

            builder.Property(e => e.IdRole)
                .HasColumnName("id_role")
                .HasColumnType("int(11)");

            builder.Property(e => e.RoleName)
                .IsRequired()
                .HasColumnName("role_name")
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}
