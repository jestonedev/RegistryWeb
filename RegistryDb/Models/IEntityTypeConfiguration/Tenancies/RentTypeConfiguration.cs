using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class RentTypeConfiguration : IEntityTypeConfiguration<RentType>
    {
        private string nameDatebase;

        public RentTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<RentType> builder)
        {
            builder.HasKey(e => e.IdRentType);

            builder.ToTable("rent_types", nameDatebase);

            builder.Property(e => e.IdRentType)
                .HasColumnName("id_rent_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.RentTypeName)
                .IsRequired()
                .HasColumnName("rent_type")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.RentTypeGenetive)
                .IsRequired()
                .HasColumnName("rent_type_genetive")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.RentTypeShort)
                .IsRequired()
                .HasColumnName("rent_type_short")
                .HasMaxLength(10)
                .IsUnicode(false);
        }
    }
}
