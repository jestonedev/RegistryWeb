using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class AclUserConfiguration : IEntityTypeConfiguration<AclUser>
    {
        private string nameDatebase;

        public AclUserConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<AclUser> builder)
        {
            builder.HasKey(e => e.IdUser);

            builder.ToTable("acl_users", nameDatebase);

            builder.Property(e => e.IdUser)
                .HasColumnName("id_user")
                .HasColumnType("int(11)");

            builder.Property(e => e.UserName)
                .IsRequired()
                .HasColumnName("user_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.UserDescription)
                .HasColumnName("user_description")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
