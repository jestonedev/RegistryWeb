using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.SqlViews;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyActiveProcessConfiguration : IEntityTypeConfiguration<TenancyActiveProcess>
    {
        private string nameDatebase;

        public TenancyActiveProcessConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyActiveProcess> builder)
        {
            builder.HasKey(e => e.IdProcess);

            builder.ToTable("v_tenancy_active_processes", nameDatebase);

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasMaxLength(17)
                .IsUnicode(false);
            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasMaxLength(17)
                .IsUnicode(false);
            builder.Property(e => e.IdSubPremises)
                .HasColumnName("id_sub_premises")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.Tenants)
                .HasColumnName("tenants")
                .IsUnicode(false);

            builder.Property(e => e.CountTenants)
                .HasColumnName("count_tenants")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.HasOne(d => d.TenancyProcessNavigation)
                .WithOne(p => p.TenancyActiveContractNavigation)
                .HasForeignKey<TenancyActiveProcess>(d => d.IdProcess);
        }
    }
}
