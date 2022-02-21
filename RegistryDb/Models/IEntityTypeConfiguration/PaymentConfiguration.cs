using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        private string nameDatebase;

        public PaymentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(e => e.IdPayment);

            builder.ToTable("payments", nameDatebase);

            builder.Property(e => e.IdPayment)
                .HasColumnName("id_payment")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("datetime");

            builder.Property(e => e.Tenant)
                .HasColumnName("tenant")
                .HasMaxLength(255)
                .IsUnicode(false);
            
            builder.Property(e => e.TotalArea)
                .HasColumnName("total_area")
                .HasColumnType("double");

            builder.Property(e => e.LivingArea)
                .HasColumnName("living_area")
                .HasColumnType("double");

            builder.Property(e => e.Prescribed)
                .HasColumnName("prescribed")
                .HasColumnType("int(11)");

            builder.Property(e => e.BalanceInput)
                .HasColumnName("balance_input")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceTenancy)
                .HasColumnName("balance_tenancy")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceDgi)
                .HasColumnName("balance_dgi")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalancePadun)
                .HasColumnName("balance_padun")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalancePkk)
                .HasColumnName("balance_pkk")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceInputPenalties)
                .HasColumnName("balance_input_penalties")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.ChargingTenancy)
                .HasColumnName("charging_tenancy")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.ChargingTotal)
                .HasColumnName("charging_total")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.ChargingDgi)
                .HasColumnName("charging_dgi")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.ChargingPadun)
                .HasColumnName("charging_padun")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.ChargingPkk)
                .HasColumnName("charging_pkk")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.ChargingPenalties)
                .HasColumnName("charging_penalties")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.RecalcTenancy)
                .HasColumnName("recalc_tenancy")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.RecalcDgi)
                .HasColumnName("recalc_dgi")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.RecalcPadun)
                .HasColumnName("recalc_padun")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.RecalcPkk)
                .HasColumnName("recalc_pkk")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.RecalcPenalties)
                .HasColumnName("recalc_penalties")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.PaymentTenancy)
                .HasColumnName("payment_tenancy")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.PaymentDgi)
                .HasColumnName("payment_dgi")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.PaymentPadun)
                .HasColumnName("payment_padun")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.PaymentPkk)
                .HasColumnName("payment_pkk")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.PaymentPenalties)
                .HasColumnName("payment_penalties")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.TransferBalance)
                .HasColumnName("transfer_balance")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceOutputTotal)
                .HasColumnName("balance_output_total")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceOutputTenancy)
                .HasColumnName("balance_output_tenancy")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceOutputDgi)
                .HasColumnName("balance_output_dgi")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceOutputPadun)
                .HasColumnName("balance_output_padun")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceOutputPkk)
                .HasColumnName("balance_output_pkk")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.BalanceOutputPenalties)
                .HasColumnName("balance_output_penalties")
                .HasColumnType("decimal(12,2)");
        }
    }
}
