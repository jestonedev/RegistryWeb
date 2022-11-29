using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Kladr
{
    public class KladrStreetConfiguration : IEntityTypeConfiguration<KladrStreet>
    {
        private string nameDatebase;

        public KladrStreetConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KladrStreet> builder)
        {
            builder.HasKey(e => e.IdStreet);

            builder.ToTable("v_kladr_streets", nameDatebase);

            builder.Property(e => e.IdStreet)
                .HasColumnName("id_street")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.StreetName)
                .HasColumnName("street_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.StreetLong)
                .HasColumnName("street_long")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
