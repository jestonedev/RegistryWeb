using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class AclPrivilegeTypeConfiguration : IEntityTypeConfiguration<AclPrivilegeType>
    {
        private string nameDatebase;

        public AclPrivilegeTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<AclPrivilegeType> builder)
        {
            builder.HasKey(e => e.IdPrivilegeType);

            builder.ToTable("acl_privilege_type", nameDatebase);

            builder.Property(e => e.IdPrivilegeType)
                .HasColumnName("id_privilege_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.PrivilegeType)
                .IsRequired()
                .HasColumnName("privilege_type")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
