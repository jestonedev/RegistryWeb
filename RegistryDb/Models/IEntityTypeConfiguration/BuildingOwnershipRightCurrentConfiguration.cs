using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.SqlViews;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class BuildingOwnershipRightCurrentConfiguration : IEntityTypeConfiguration<BuildingOwnershipRightCurrent>
    {
        private string nameDatebase;

        public BuildingOwnershipRightCurrentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<BuildingOwnershipRightCurrent> builder)
        {
            builder.HasKey(e => e.IdBuilding);

            builder.ToTable("v_buildings_ownership_rights_1_current", nameDatebase);

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.OwnershipRightType)
                .HasColumnName("ownership_right_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IdOwnershipRight)
                .HasColumnName("id_ownership_right")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwnershipRightType)
                .HasColumnName("id_ownership_right_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Number)
                .HasColumnName("number")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ResettlePlanDate)
                .HasColumnName("resettle_plan_date")
                .HasColumnType("date");

            builder.Property(e => e.DemolishPlanDate)
                .HasColumnName("demolish_plan_date")
                .HasColumnType("date");
        }
    }
}
