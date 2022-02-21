using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PremisesKindConfiguration : IEntityTypeConfiguration<PremisesKind>
    {
        private string nameDatebase;

        public PremisesKindConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PremisesKind> builder)
        {
            builder.HasKey(e => e.IdPremisesKind);

            builder.ToTable("premises_kinds", nameDatebase);

            builder.Property(e => e.IdPremisesKind)
                .HasColumnName("id_premises_kind")
                .HasColumnType("int(11)");

            builder.Property(e => e.PremisesKindName)
                .HasColumnName("premises_kind")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
