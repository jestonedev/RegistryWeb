using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class OwnerBuildingAssocConfiguration : IEntityTypeConfiguration<OwnerBuildingAssoc>
    {
        private string nameDatebase;

        public OwnerBuildingAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerBuildingAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("owner_buildings_assoc", nameDatebase);

            builder.HasIndex(e => e.IdBuilding)
                .HasName("FK_owner_buildings_assoc_id_building");

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_buildings_assoc_id_process");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.OwnerBuildingsAssoc)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_owner_buildings_assoc_id_process");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
