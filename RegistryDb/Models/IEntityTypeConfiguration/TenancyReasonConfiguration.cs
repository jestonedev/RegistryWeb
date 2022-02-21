using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class TenancyReasonConfiguration : IEntityTypeConfiguration<TenancyReason>
    {
        private string nameDatebase;

        public TenancyReasonConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyReason> builder)
        {
            builder.HasKey(e => e.IdReason);

            builder.ToTable("tenancy_reasons", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_contract_reasons_tenancy_contracts_id_contract");

            builder.HasIndex(e => e.IdReasonType)
                .HasName("FK_contract_reasons_premises_types_id_premises_type");

            builder.Property(e => e.IdReason)
                .HasColumnName("id_reason")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdReasonType)
                .HasColumnName("id_reason_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.ReasonDate)
                .HasColumnName("reason_date")
                .HasColumnType("date");

            builder.Property(e => e.ReasonNumber)
                .HasColumnName("reason_number")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.ReasonPrepared)
                .IsRequired()
                .HasColumnName("reason_prepared")
                .IsUnicode(false);

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.TenancyReasons)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_contract_reasons_tenancy_contracts_id_contract");

            builder.HasOne(d => d.IdReasonTypeNavigation)
                .WithMany(p => p.TenancyReasons)
                .HasForeignKey(d => d.IdReasonType)
                .HasConstraintName("FK_contract_reasons_premises_types_id_premises_type");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
