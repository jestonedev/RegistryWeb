using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class KumiPaymentReasonConfiguration : IEntityTypeConfiguration<KumiPaymentReason>
    {
        private string nameDatebase;

        public KumiPaymentReasonConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentReason> builder)
        {
            builder.HasKey(e => e.IdPaymentReason);

            builder.ToTable("kumi_payment_reasons", nameDatebase);

            builder.Property(e => e.IdPaymentReason)
                .HasColumnName("id_payment_reason")
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
