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

            builder.Property(e => e.Value)
                .HasColumnName("value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.HasOne(e => e.Payment).WithMany(e => e.PaymentCharges)
                .HasForeignKey(e => e.IdPayment);

            builder.HasOne(e => e.Charge).WithMany(e => e.PaymentCharges)
                .HasForeignKey(e => e.IdCharge);
        }
    }
}
