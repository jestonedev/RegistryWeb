using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentChargeConfiguration : IEntityTypeConfiguration<KumiPaymentCharge>
    {
        private string nameDatebase;

        public KumiPaymentChargeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentCharge> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("kumi_payments_charges", nameDatebase);

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdPayment)
                .HasColumnName("id_payment")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdCharge)
                .HasColumnName("id_charge")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date")
                .IsRequired();

            // Найм
            builder.Property(e => e.TenancyValue)
                .HasColumnName("tenancy_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PenaltyValue)
                .HasColumnName("penalty_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // ДГИ
            builder.Property(e => e.DgiValue)
                .HasColumnName("dgi_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // ПКК
            builder.Property(e => e.PkkValue)
                .HasColumnName("pkk_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // Падун
            builder.Property(e => e.PadunValue)
                .HasColumnName("padun_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.IdDisplayCharge)
                .HasColumnName("id_display_charge")
                .HasColumnType("int(11)");

            builder.HasOne(e => e.Payment).WithMany(e => e.PaymentCharges)
                .HasForeignKey(e => e.IdPayment);

            builder.HasOne(e => e.Charge).WithMany(e => e.PaymentCharges)
                .HasForeignKey(e => e.IdCharge);

            builder.HasOne(e => e.DisplayCharge).WithMany(e => e.DisplayPaymentCharges)
                .HasForeignKey(e => e.IdDisplayCharge);
        }
    }
}
