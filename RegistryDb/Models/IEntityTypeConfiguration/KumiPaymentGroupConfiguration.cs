using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class KumiPaymentGroupConfiguration : IEntityTypeConfiguration<KumiPaymentGroup>
    {
        private string nameDatebase;

        public KumiPaymentGroupConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentGroup> builder)
        {
            builder.HasKey(e => e.IdGroup);

            builder.ToTable("kumi_payment_groups", nameDatebase);

            builder.Property(e => e.IdGroup)
                .HasColumnName("id_group")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.User)
                .HasColumnName("user")
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasMany(e => e.PaymentGroupFiles)
                .WithOne(e => e.PaymentGroup)
                .HasForeignKey(e => e.IdGroup);
        }
    }
}
