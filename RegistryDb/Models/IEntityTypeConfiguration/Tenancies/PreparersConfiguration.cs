using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class PreparersConfiguration: IEntityTypeConfiguration<Preparer>
    {
        private string nameDatebase;

        public PreparersConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Preparer> builder)
        {
            builder.ToTable("preparers", nameDatebase);

            builder.HasKey(e => e.IdPreparer);

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
