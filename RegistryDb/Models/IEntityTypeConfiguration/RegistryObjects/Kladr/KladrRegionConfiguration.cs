using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.SqlViews;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Kladr
{
    public class KladrRegionConfiguration : IEntityTypeConfiguration<KladrRegion>
    {
        private string nameDatebase;

        public KladrRegionConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KladrRegion> builder)
        {
            builder.HasKey(e => e.IdRegion);

            builder.ToTable("v_kladr_regions", nameDatebase);

            builder.Property(e => e.IdRegion)
                .HasColumnName("id_region")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.Region)
                .HasColumnName("region")
                .HasMaxLength(40)
                .IsUnicode(false);
        }
    }
}
