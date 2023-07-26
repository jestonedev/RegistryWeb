using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiChargeCorrectionConfiguration : IEntityTypeConfiguration<KumiChargeCorrection>
    {
        private string nameDatebase;

        public KumiChargeCorrectionConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiChargeCorrection> builder)
        {
            builder.HasKey(e => e.IdCorrection);

            builder.ToTable("kumi_charges_corrections", nameDatebase);

            builder.Property(e => e.IdCorrection)
                .HasColumnName("id_correction")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1024)
                .IsUnicode(false);

            // Найм
            builder.Property(e => e.TenancyValue)
                .HasColumnName("tenancy_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PenaltyValue)
                .HasColumnName("penalty_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentTenancyValue)
                .HasColumnName("payment_tenancy_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentPenaltyValue)
                .HasColumnName("payment_penalty_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // ДГИ
            builder.Property(e => e.DgiValue)
                .HasColumnName("dgi_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentDgiValue)
                .HasColumnName("payment_dgi_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // ПКК
            builder.Property(e => e.PkkValue)
                .HasColumnName("pkk_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentPkkValue)
                .HasColumnName("payment_pkk_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            // Падун
            builder.Property(e => e.PadunValue)
                .HasColumnName("padun_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentPadunValue)
                .HasColumnName("payment_padun_value")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.User)
                .HasColumnName("user")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(e => e.Account)
                .WithMany(e => e.Corrections)
                .HasForeignKey(e => e.IdAccount);
        }
    }
}
