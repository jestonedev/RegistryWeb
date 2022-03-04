using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class KumiPaymentKindConfiguration : IEntityTypeConfiguration<KumiPaymentKind>
    {
        private string nameDatebase;

        public KumiPaymentKindConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentKind> builder)
        {
            builder.HasKey(e => e.IdPaymentKind);

            builder.ToTable("kumi_payment_kinds", nameDatebase);

            builder.Property(e => e.IdPaymentKind)
                .HasColumnName("id_payment_kind")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Code)
                .HasColumnName("code")
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false);
        }
    }
}
