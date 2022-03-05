using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentCorrectionConfiguration : IEntityTypeConfiguration<KumiPaymentCorrection>
    {
        private string nameDatebase;

        public KumiPaymentCorrectionConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentCorrection> builder)
        {
            builder.HasKey(e => e.IdCorrection);

            builder.ToTable("kumi_payments_corrections", nameDatebase);

            builder.Property(e => e.IdCorrection)
                .HasColumnName("id_correction")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdPayment)
                .HasColumnName("id_payment")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.FieldName)
                .HasColumnName("field_name")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.FieldValue)
                .HasColumnName("field_value")
                .HasMaxLength(500)
                .IsUnicode(false);

            builder.HasOne(e => e.Payment)
                .WithMany(e => e.PaymentCorrections)
                .HasForeignKey(e => e.IdCorrection);
        }
    }
}
