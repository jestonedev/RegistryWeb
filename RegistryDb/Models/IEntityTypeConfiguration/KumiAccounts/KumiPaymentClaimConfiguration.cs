using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentClaimConfiguration : IEntityTypeConfiguration<KumiPaymentClaim>
    {
        private string nameDatebase;

        public KumiPaymentClaimConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentClaim> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("kumi_payments_claims", nameDatebase);

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdPayment)
                .HasColumnName("id_payment")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
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

            builder.HasOne(e => e.Payment).WithMany(e => e.PaymentClaims)
                .HasForeignKey(e => e.IdPayment);

            builder.HasOne(e => e.Claim).WithMany(e => e.PaymentClaims)
                .HasForeignKey(e => e.IdClaim);
        }
    }
}
