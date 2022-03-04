using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class KumiPaymentUfConfiguration : IEntityTypeConfiguration<KumiPaymentUf>
    {
        private string nameDatebase;

        public KumiPaymentUfConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentUf> builder)
        {
            builder.HasKey(e => e.IdPaymentUf);

            builder.ToTable("kumi_payments_uf", nameDatebase);

            builder.Property(e => e.IdPaymentUf)
                .HasColumnName("id_payment_uf")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdPayment)
                .HasColumnName("id_payment")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.NumUf)
                .HasColumnName("num_uf")
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.DateUf)
                .HasColumnName("date_uf")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.Sum)
                .HasColumnName("sum")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.Purpose)
                .HasColumnName("purpose")
                .HasMaxLength(500)
                .IsUnicode(false);

            builder.Property(e => e.Kbk)
                .HasColumnName("kbk")
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.IdKbkType)
                .HasColumnName("id_kbk_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.TargetCode)
                .HasColumnName("target_code")
                .HasMaxLength(25)
                .IsUnicode(false);

            builder.Property(e => e.Okato)
                .HasColumnName("okato")
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.RecipientInn)
               .HasColumnName("recipient_inn")
               .HasMaxLength(12)
               .IsUnicode(false);

            builder.Property(e => e.RecipientKpp)
               .HasColumnName("recipient_kpp")
               .HasMaxLength(12)
               .IsUnicode(false);

            builder.Property(e => e.RecipientName)
               .HasColumnName("recipient_name")
               .HasMaxLength(160)
               .IsUnicode(false);

            builder.Property(e => e.RecipientAccount)
               .HasColumnName("recipient_account")
               .HasMaxLength(20)
               .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.HasOne(e => e.KbkType)
                .WithMany(e => e.Paymentufs)
                .HasForeignKey(e => e.IdKbkType);

            builder.HasQueryFilter(r => r.Deleted == 0);
        }
    }
}
