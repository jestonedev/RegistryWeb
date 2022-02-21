using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PremisesDoorKeysConfiguration : IEntityTypeConfiguration<PremisesDoorKeys>
    {
        private string nameDatebase;

        public PremisesDoorKeysConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PremisesDoorKeys> builder)
        {
            builder.HasKey(e => e.IdPremisesDoorKeys);

            builder.ToTable("premises_door_keys", nameDatebase);

            builder.Property(e => e.IdPremisesDoorKeys)
                .HasColumnName("id_premises_door_keys")
                .HasColumnType("int(11)");

            builder.Property(e => e.LocationOfKeys)
                .IsRequired()
                .HasColumnName("location_of_keys")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
