using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Buildings.Litigations;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Buildings.Litigations
{
    public class LitigationPremiseAssocConfiguration : IEntityTypeConfiguration<LitigationPremiseAssoc>
    {
        private string nameDatebase;

        public LitigationPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<LitigationPremiseAssoc> builder)
        {
            builder.HasKey(e => new { e.IdPremises, e.IdLitigation });
            
            builder.ToTable("litigation_premise_assoc", nameDatebase);

            builder.HasIndex(e => e.IdLitigation)
                .HasName("FK_litigation_premise_assoc_id_litigation_info");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdLitigation)
                .HasColumnName("id_litigation")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.LitigationNavigation)
                .WithMany(p => p.LitigationPremisesAssoc)
                .HasForeignKey(d => d.IdLitigation)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.PremiseNavigation)
                .WithMany(p => p.LitigationPremisesAssoc)
                .HasForeignKey(d => d.IdPremises)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
