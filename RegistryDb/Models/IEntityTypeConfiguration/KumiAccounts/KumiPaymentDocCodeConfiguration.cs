using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentDocCodeConfiguration : IEntityTypeConfiguration<KumiPaymentDocCode>
    {
        private string nameDatebase;

        public KumiPaymentDocCodeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentDocCode> builder)
        {
            builder.HasKey(e => e.IdPaymentDocCode);

            builder.ToTable("kumi_payment_doc_codes", nameDatebase);

            builder.Property(e => e.IdPaymentDocCode)
                .HasColumnName("id_payment_doc_code")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Code)
                .HasColumnName("code")
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false);
        }
    }
}
