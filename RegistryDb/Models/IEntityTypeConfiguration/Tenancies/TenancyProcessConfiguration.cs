using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyProcessConfiguration : IEntityTypeConfiguration<TenancyProcess>
    {
        private string nameDatebase;

        public TenancyProcessConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyProcess> builder)
        {
            builder.HasKey(e => e.IdProcess);

            builder.ToTable("tenancy_processes", nameDatebase);

            builder.HasIndex(e => e.IdExecutor)
                .HasName("FK_tenancy_contracts_executors_id_executor");

            builder.HasIndex(e => e.IdRentType)
                .HasName("FK_tenancy_contracts_rent_types_id_rent_type");

            builder.HasIndex(e => e.IdRentTypeCategory)
                .HasName("FK_tenancy_processes_id_rent_type_category");

            builder.HasIndex(e => e.IdWarrant)
                .HasName("FK_tenancy_contracts_warrants_id_warrant");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.BeginDate)
                .HasColumnName("begin_date")
                .HasColumnType("date");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsUnicode(false);

            builder.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date");

            builder.Property(e => e.IdExecutor)
                .HasColumnName("id_executor")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdRentType)
                .HasColumnName("id_rent_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdRentTypeCategory)
                .HasColumnName("id_rent_type_category")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdEmployer)
                .HasColumnName("id_employer")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdWarrant)
                .HasColumnName("id_warrant")
                .HasColumnType("int(11)");

            builder.Property(e => e.IssueDate)
                .HasColumnName("issue_date")
                .HasColumnType("date");

            builder.Property(e => e.AnnualDate)
               .HasColumnName("annual_date")
               .HasColumnType("date");

            builder.Property(e => e.ProtocolDate)
                .HasColumnName("protocol_date")
                .HasColumnType("date");

            builder.Property(e => e.ProtocolNum)
                .HasColumnName("protocol_num")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.RegistrationDate)
                .HasColumnName("registration_date")
                .HasColumnType("date");

            builder.Property(e => e.RegistrationNum)
                .HasColumnName("registration_num")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ResidenceWarrantDate)
                .HasColumnName("residence_warrant_date")
                .HasColumnType("date");

            builder.Property(e => e.ResidenceWarrantNum)
                .HasColumnName("residence_warrant_num")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.SubTenancyDate)
                .HasColumnName("sub_tenancy_date")
                .HasColumnType("date");

            builder.Property(e => e.SubTenancyNum)
                .HasColumnName("sub_tenancy_num")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.UntilDismissal)
                .HasColumnName("until_dismissal")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdRentTypeNavigation)
                .WithMany(p => p.TenancyProcesses)
                .HasForeignKey(d => d.IdRentType)
                .HasConstraintName("FK_tenancy_contracts_rent_types_id_rent_type");

            builder.HasOne(d => d.IdRentTypeCategoryNavigation)
                .WithMany(p => p.TenancyProcesses)
                .HasForeignKey(d => d.IdRentTypeCategory)
                .HasConstraintName("FK_tenancy_processes_id_rent_type_category");

            builder.HasOne(d => d.IdExecutorNavigation)
                .WithMany(p => p.TenancyProcesses)
                .HasForeignKey(d => d.IdExecutor)
                .HasConstraintName("FK_tenancy_contracts_executors_id_executor");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
