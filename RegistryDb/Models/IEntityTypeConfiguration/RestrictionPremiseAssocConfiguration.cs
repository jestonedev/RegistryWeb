using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class RestrictionPremiseAssocConfiguration : IEntityTypeConfiguration<RestrictionPremiseAssoc>
    {
        private string nameDatebase;

        public RestrictionPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<RestrictionPremiseAssoc> builder)
        {
            builder.HasKey(e => new { e.IdPremises, e.IdRestriction });
            
            builder.ToTable("restrictions_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdRestriction)
                .HasName("FK_restrictions_premises_assoc_restrictions_id_restriction");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdRestriction)
                .HasColumnName("id_restriction")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.RestrictionNavigation)
                .WithMany(p => p.RestrictionPremisesAssoc)
                .HasForeignKey(d => d.IdRestriction)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.PremisesNavigation)
                .WithMany(p => p.RestrictionPremisesAssoc)
                .HasForeignKey(d => d.IdPremises)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
