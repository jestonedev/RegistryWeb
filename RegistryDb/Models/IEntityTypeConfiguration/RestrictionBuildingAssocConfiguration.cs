using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class RestrictionBuildingAssocConfiguration : IEntityTypeConfiguration<RestrictionBuildingAssoc>
    {
        private string nameDatebase;

        public RestrictionBuildingAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<RestrictionBuildingAssoc> builder)
        {
            builder.HasKey(e => new { e.IdBuilding, e.IdRestriction });

            builder.ToTable("restrictions_buildings_assoc", nameDatebase);

            builder.HasIndex(e => e.IdRestriction)
                .HasName("FK_restrictions_buildings_assoc_restrictions_id_restriction");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdRestriction)
                .HasColumnName("id_restriction")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.RestrictionNavigation)
                .WithMany(p => p.RestrictionBuildingsAssoc)
                .HasForeignKey(d => d.IdRestriction)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
