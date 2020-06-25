using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class TotalAreaAvgCostConfiguration : IEntityTypeConfiguration<TotalAreaAvgCost>
    {
        private string nameDatebase;

        public TotalAreaAvgCostConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TotalAreaAvgCost> builder)
        {
            builder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            
            builder.ToTable("total_area_avg_cost", "registry_test");

            builder.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

            builder.Property(e => e.Cost)
                    .HasColumnName("cost")
                    .HasColumnType("decimal(19,2)");
            
        }
    }
}
