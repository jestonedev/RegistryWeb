using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentConfiguration : IEntityTypeConfiguration<KumiPayment>
    {
        private string nameDatebase;

        public KumiPaymentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPayment> builder)
        {
            builder.HasKey(e => e.IdPayment);

            builder.ToTable("kumi_payments", nameDatebase);

            builder.Property(e => e.IdPayment)
                .HasColumnName("id_payment")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdParentPayment)
                .HasColumnName("id_parent_payment")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdGroup)
                .HasColumnName("id_group")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSource)
                .HasColumnName("id_source")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasMaxLength(36)
                .IsUnicode(false);

            builder.Property(e => e.IdPaymentDocCode)
                .HasColumnName("id_payment_doc_code")
                .HasColumnType("int(11)");

            builder.Property(e => e.NumDocument)
                .HasColumnName("num_d")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.DateDocument)
                .HasColumnName("date_d")
                .HasColumnType("date");

            builder.Property(e => e.DateIn)
                .HasColumnName("date_in")
                .HasColumnType("date");

            builder.Property(e => e.DateExecute)
                .HasColumnName("date_e")
                .HasColumnType("date");

            builder.Property(e => e.DatePay)
                .HasColumnName("date_pay")
                .HasColumnType("date");

            builder.Property(e => e.IdPaymentKind)
                .HasColumnName("id_payment_kind")
                .HasColumnType("int(11)");

            builder.Property(e => e.OrderPay)
                .HasColumnName("order_pay")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOperationType)
                .HasColumnName("id_operation_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Sum)
                .HasColumnName("sum")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.Uin)
                .HasColumnName("uin")
                .HasMaxLength(25)
                .IsUnicode(false);

            builder.Property(e => e.IdPurpose)
                .HasColumnName("id_purpose")
                .HasColumnType("int(11)");

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

            builder.Property(e => e.IdPaymentReason)
                .HasColumnName("id_payment_reason")
                .HasColumnType("int(11)");

            builder.Property(e => e.NumDocumentIndicator)
               .HasColumnName("num_d_indicator")
               .HasMaxLength(15)
               .IsUnicode(false);

            builder.Property(e => e.DateDocumentIndicator)
                .HasColumnName("date_d_indicator")
                .HasColumnType("date");

            builder.Property(e => e.IdPayerStatus)
                .HasColumnName("id_payer_status")
                .HasColumnType("int(11)");

            builder.Property(e => e.PayerInn)
               .HasColumnName("payer_inn")
               .HasMaxLength(12)
               .IsUnicode(false);

            builder.Property(e => e.PayerKpp)
               .HasColumnName("payer_kpp")
               .HasMaxLength(12)
               .IsUnicode(false);

            builder.Property(e => e.PayerName)
               .HasColumnName("payer_name")
               .HasMaxLength(2000)
               .IsUnicode(false);

            builder.Property(e => e.PayerAccount)
               .HasColumnName("payer_account")
               .HasMaxLength(20)
               .IsUnicode(false);

            builder.Property(e => e.PayerBankBik)
               .HasColumnName("payer_bank_bik")
               .HasMaxLength(9)
               .IsUnicode(false);

            builder.Property(e => e.PayerBankName)
               .HasColumnName("payer_bank_name")
               .HasMaxLength(160)
               .IsUnicode(false);

            builder.Property(e => e.PayerBankAccount)
               .HasColumnName("payer_bank_account")
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
               .HasMaxLength(2000)
               .IsUnicode(false);

            builder.Property(e => e.RecipientAccount)
               .HasColumnName("recipient_account")
               .HasMaxLength(20)
               .IsUnicode(false);

            builder.Property(e => e.RecipientBankBik)
               .HasColumnName("recipient_bank_bik")
               .HasMaxLength(9)
               .IsUnicode(false);

            builder.Property(e => e.RecipientBankName)
               .HasColumnName("recipient_bank_name")
               .HasMaxLength(160)
               .IsUnicode(false);

            builder.Property(e => e.RecipientBankAccount)
               .HasColumnName("recipient_bank_account")
               .HasMaxLength(20)
               .IsUnicode(false);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1024)
                .IsUnicode(false);

            builder.Property(e => e.DateEnrollUfk)
                .HasColumnName("date_enroll_ufk")
                .HasColumnType("date");

            builder.Property(e => e.IsPosted)
                .HasColumnName("is_posted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.HasOne(e => e.PaymentGroup)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.IdGroup);

            builder.HasOne(e => e.PaymentInfoSource)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.IdSource);

            builder.HasOne(e => e.PaymentDocCode)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.IdPaymentDocCode);

            builder.HasOne(e => e.PaymentKind)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.IdPaymentKind);

            builder.HasOne(e => e.OperationType)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.IdOperationType);

            builder.HasOne(e => e.KbkType)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.IdKbkType);

            builder.HasOne(e => e.PaymentReason)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.IdPaymentReason);

            builder.HasOne(e => e.PayerStatus)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.IdPayerStatus);

            builder.HasOne(e => e.ParentPayment)
                .WithMany(e => e.ChildPayments)
                .HasForeignKey(e => e.IdParentPayment);

            builder.HasQueryFilter(r => r.Deleted == 0);
        }
    }
}
