using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentInfoSourceConfiguration : IEntityTypeConfiguration<KumiPaymentInfoSource>
    {
        private string nameDatebase;

        public KumiPaymentInfoSourceConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentInfoSource> builder)
        {
            builder.HasKey(e => e.IdSource);

            builder.ToTable("kumi_payment_info_sources", nameDatebase);

            builder.Property(e => e.IdSource)
                .HasColumnName("id_source")
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
                .HasMaxLength(10)
                .IsUnicode(false);
        }
    }
}
