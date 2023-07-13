using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiChargeConfiguration : IEntityTypeConfiguration<KumiCharge>
    {
        private string nameDatebase;

        public KumiChargeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiCharge> builder)
        {
            builder.HasKey(e => e.IdCharge);

            builder.ToTable("kumi_charges", nameDatebase);

            builder.Property(e => e.IdCharge)
                .HasColumnName("id_charge")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date")
                .IsRequired();

            // Найм
            builder.Property(e => e.InputTenancy)
                .HasColumnName("input_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.InputPenalty)
                .HasColumnName("input_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.ChargeTenancy)
                .HasColumnName("charge_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.ChargePenalty)
                .HasColumnName("charge_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentTenancy)
                .HasColumnName("payment_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentPenalty)
                .HasColumnName("payment_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.RecalcTenancy)
                .HasColumnName("recalc_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.RecalcPenalty)
                .HasColumnName("recalc_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.CorrectionTenancy)
                .HasColumnName("correction_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.CorrectionPenalty)
                .HasColumnName("correction_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.OutputTenancy)
                .HasColumnName("output_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.OutputPenalty)
                .HasColumnName("output_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // ДГИ
            builder.Property(e => e.InputDgi)
                .HasColumnName("input_dgi")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.ChargeDgi)
                .HasColumnName("charge_dgi")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentDgi)
                .HasColumnName("payment_dgi")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.RecalcDgi)
                .HasColumnName("recalc_dgi")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.CorrectionDgi)
                .HasColumnName("correction_dgi")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.OutputDgi)
                .HasColumnName("output_dgi")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // ПКК
            builder.Property(e => e.InputPkk)
                .HasColumnName("input_pkk")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.ChargePkk)
                .HasColumnName("charge_pkk")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentPkk)
                .HasColumnName("payment_pkk")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.RecalcPkk)
                .HasColumnName("recalc_pkk")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.CorrectionPkk)
                .HasColumnName("correction_pkk")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.OutputPkk)
                .HasColumnName("output_pkk")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // Падун
            builder.Property(e => e.InputPadun)
                .HasColumnName("input_padun")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.ChargePadun)
                .HasColumnName("charge_padun")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentPadun)
                .HasColumnName("payment_padun")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.RecalcPadun)
                .HasColumnName("recalc_padun")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.CorrectionPadun)
                .HasColumnName("correction_padun")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.OutputPadun)
                .HasColumnName("output_padun")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.Hidden)
                .HasColumnName("hidden")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.Property(e => e.IsBksCharge)
                .HasColumnName("is_bks_charge")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.HasOne(e => e.Account).WithMany(e => e.Charges)
                .HasForeignKey(e => e.IdAccount);

            builder.HasQueryFilter(r => r.Deleted == 0);
        }
    }
}
