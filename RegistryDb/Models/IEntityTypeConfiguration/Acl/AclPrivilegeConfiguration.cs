using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities.Acl;

namespace RegistryDb.Models.IEntityTypeConfiguration.Acl
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
                .HasColumnType("bigint(20)");

            builder.Property(e => e.PrivilegeDescription)
                .HasColumnName("privilege_description")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
