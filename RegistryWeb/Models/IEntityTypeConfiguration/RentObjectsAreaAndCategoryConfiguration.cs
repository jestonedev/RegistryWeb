using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class RentObjectsAreaAndCategoryConfiguration: IEntityTypeConfiguration<RentObjectsAreaAndCategory>
    {
        private string nameDatebase;

        public RentObjectsAreaAndCategoryConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<RentObjectsAreaAndCategory> builder)
        {
            builder.HasKey(e => new {
                e.IdProcess, e.IdPremises, e.IdSubPremises
            });

            builder.ToTable("v_rent_objects_area_and_categories", nameDatebase);

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSubPremises)
                .HasColumnName("id_sub_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.RentArea)
                .HasColumnName("rent_area")
                .HasDefaultValueSql("0");

            builder.Property(e => e.RentCoefficient)
                .HasColumnName("rent_coefficient")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdRentCategory)
                .HasColumnName("id_rent_category")
                .HasColumnType("int(11)");

        }
    }
}
