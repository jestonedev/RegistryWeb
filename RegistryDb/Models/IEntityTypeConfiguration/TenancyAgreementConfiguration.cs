using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class TenancyAgreementConfiguration : IEntityTypeConfiguration<TenancyAgreement>
    {
        private string nameDatebase;

        public TenancyAgreementConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyAgreement> builder)
        {
            builder.HasKey(e => e.IdAgreement);

            builder.ToTable("tenancy_agreements", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_agreements_tenancy_contracts_id_contract");

            builder.HasIndex(e => e.IdExecutor)
                .HasName("FK_agreements_executors_id_executor");

            builder.Property(e => e.IdAgreement)
                .HasColumnName("id_agreement")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.AgreementDate)
                .HasColumnName("agreement_date")
                .HasColumnType("date");

            builder.Property(e => e.AgreementContent)
                .HasColumnName("agreement_content")
                .IsUnicode(false);

            builder.Property(e => e.IdExecutor)
                .HasColumnName("id_executor")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdWarrant)
                .HasColumnName("id_warrant")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.TenancyAgreements)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_agreements_tenancy_contracts_id_contract");

            builder.HasOne(d => d.IdExecutorNavigation)
                .WithMany(p => p.TenancyAgreements)
                .HasForeignKey(d => d.IdExecutor)
                .HasConstraintName("FK_agreements_executors_id_executor");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
