using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class RentPremiseConfiguration : IEntityTypeConfiguration<RentPremise>
    {
        private string nameDatebase;

        public RentPremiseConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<RentPremise> builder)
        {
            builder.HasKey(e => e.IdPremises);

            builder.ToTable("v_rent_premises", nameDatebase);

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.Payment)
                .HasColumnName("payment")
                .HasDefaultValueSql("0");
        }
    }
}
