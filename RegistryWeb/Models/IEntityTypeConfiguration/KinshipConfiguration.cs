using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class KinshipConfiguration : IEntityTypeConfiguration<Kinship>
    {
        private string nameDatebase;

        public KinshipConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Kinship> builder)
        {
            builder.HasKey(e => e.IdKinship);

            builder.ToTable("kinships", nameDatebase);

            builder.Property(e => e.IdKinship)
                .HasColumnName("id_kinship")
                .HasColumnType("int(11)");

            builder.Property(e => e.KinshipName)
                .IsRequired()
                .HasColumnName("kinship")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
