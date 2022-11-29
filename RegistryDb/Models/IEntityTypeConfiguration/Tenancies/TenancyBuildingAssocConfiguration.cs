using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyBuildingAssocConfiguration : IEntityTypeConfiguration<TenancyBuildingAssoc>
    {
        private string nameDatebase;

        public TenancyBuildingAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyBuildingAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("tenancy_buildings_assoc", nameDatebase);

            builder.HasIndex(e => e.IdBuilding)
                .HasName("FK_tenancy_buildings_assoc_buildings_id_building");

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_tenancy_buildings_assoc_tenancy_contracts_id_contract");

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

            builder.Property(e => e.RentLivingArea).HasColumnName("rent_living_area");

            builder.Property(e => e.RentTotalArea).HasColumnName("rent_total_area");

            builder.HasOne(d => d.BuildingNavigation)
                .WithMany(p => p.TenancyBuildingsAssoc)
                .HasForeignKey(d => d.IdBuilding);

            builder.HasOne(d => d.ProcessNavigation)
                .WithMany(p => p.TenancyBuildingsAssoc)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_tenancy_buildings_assoc_tenancy_contracts_id_contract");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
