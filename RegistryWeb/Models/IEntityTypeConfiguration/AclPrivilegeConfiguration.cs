using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class AclPrivilegeConfiguration : IEntityTypeConfiguration<AclPrivilege>
    {
        private string nameDatebase;

        public AclPrivilegeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<AclPrivilege> builder)
        {
            builder.HasKey(e => e.IdPrivilege);

            builder.ToTable("acl_privileges", nameDatebase);

            builder.Property(e => e.IdPrivilege)
                .HasColumnName("id_privilege")
                .HasColumnType("int(11)");

            builder.Property(e => e.PrivilegeName)
                .HasColumnName("privilege_name")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.PrivilegeMask)
                .HasColumnName("privilege_mask")
                .HasColumnType("int(11)");

            builder.Property(e => e.PrivilegeDescription)
                .HasColumnName("privilege_description")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
