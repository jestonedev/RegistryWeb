using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PreparersConfiguration: IEntityTypeConfiguration<Preparers>
    {
        private string nameDatebase;

        public PreparersConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Preparers> builder)
        {
            builder.ToTable("preparers", nameDatebase);

            builder.HasKey(e => e.IdPreparer);

            builder.ToTable("preparers", "registry_test");

            builder.Property(e => e.IdPreparer)
                    .HasColumnName("id_preparer")
                    .HasColumnType("int(11)");

            builder.Property(e => e.Position)
                    .HasColumnName("position")
                    .HasMaxLength(255)
                    .IsUnicode(false);

            builder.Property(e => e.PreparerName)
                    .IsRequired()
                    .HasColumnName("preparer_name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

            builder.Property(e => e.ShortPosition)
                    .HasColumnName("short_position")
                    .HasMaxLength(255)
                    .IsUnicode(false);
        }  
    }
}
