using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class RentTypeCategoryConfiguration : IEntityTypeConfiguration<RentTypeCategory>
    {
        private string nameDatebase;

        public RentTypeCategoryConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<RentTypeCategory> builder)
        {
            builder.HasKey(e => e.IdRentTypeCategory);

            builder.ToTable("rent_type_categories", nameDatebase);

            builder.HasIndex(e => e.IdRentType)
                .HasName("FK_rent_type_categories_id_rent_type");

            builder.Property(e => e.IdRentTypeCategory)
                .HasColumnName("id_rent_type_category")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdRentType)
                .HasColumnName("id_rent_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.RentTypeCategoryName)
                .IsRequired()
                .HasColumnName("rent_type_category")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(d => d.IdRentTypeNavigation)
                .WithMany(p => p.RentTypeCategories)
                .HasForeignKey(d => d.IdRentType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rent_type_categories_id_rent_type");
        }
    }
}
