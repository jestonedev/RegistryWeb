using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentUntiedConfiguration : IEntityTypeConfiguration<KumiPaymentUntied>
    {
        private string nameDatebase;

        public KumiPaymentUntiedConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentUntied> builder)
        {
            builder.HasKey(e => e.IdRecord);

            builder.ToTable("kumi_payments_untied", nameDatebase);

            builder.Property(e => e.IdRecord)
                .HasColumnName("id_record")
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

            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
                .HasColumnType("int(11)");

            builder.Property(e => e.TiedDate)
                .HasColumnName("tied_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.UntiedDate)
                .HasColumnName("untied_date")
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

            builder.HasOne(e => e.Payment).WithMany(e => e.UntiedPaymentsInfo)
                .HasForeignKey(e => e.IdPayment);

            builder.HasOne(e => e.Claim).WithMany(e => e.UntiedPaymentsInfo)
                .HasForeignKey(e => e.IdClaim);

            builder.HasOne(e => e.Charge).WithMany(e => e.UntiedPaymentsInfo)
                .HasForeignKey(e => e.IdCharge);
        }
    }
}
