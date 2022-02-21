using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class OwnerOrginfoConfiguration : IEntityTypeConfiguration<OwnerOrginfo>
    {
        private string nameDatebase;

        public OwnerOrginfoConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerOrginfo> builder)
        {
            builder.HasKey(e => e.IdOwner);

            builder.ToTable("owner_orginfo", nameDatebase);

            builder.Property(e => e.IdOwner)
                .HasColumnName("id_owner")
                .HasColumnType("int(11)")
                .ValueGeneratedNever();

            builder.Property(e => e.OrgName)
                .IsRequired()
                .HasColumnName("org_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(d => d.IdOwnerNavigation)
                .WithOne(p => p.OwnerOrginfo)
                .HasForeignKey<OwnerOrginfo>(d => d.IdOwner)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
