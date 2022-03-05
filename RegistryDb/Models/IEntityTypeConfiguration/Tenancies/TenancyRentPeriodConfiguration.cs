using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyRentPeriodConfiguration : IEntityTypeConfiguration<TenancyRentPeriod>
    {
        private string nameDatebase;

        public TenancyRentPeriodConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyRentPeriod> builder)
        {
            builder.HasKey(e => e.IdRentPeriod);

            builder.ToTable("tenancy_rent_periods_history", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_tenancy_rent_periods_history_tenancy_processes_id_process");


            builder.Property(e => e.IdRentPeriod)
                .HasColumnName("id_rent_period")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.BeginDate)
                .HasColumnName("begin_date")
                .HasColumnType("date");

            builder.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date");

            builder.Property(e => e.UntilDismissal)
                .HasColumnName("until_dismissal")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.TenancyRentPeriods)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_tenancy_rent_periods_history_tenancy_processes_id_process");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
