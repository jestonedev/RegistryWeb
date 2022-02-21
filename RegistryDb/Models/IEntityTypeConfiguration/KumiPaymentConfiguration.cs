using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
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

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.Value)
                .HasColumnName("value")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.Payer)
                .HasColumnName("payer")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Purpose)
                .HasColumnName("purpose")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1024)
                .IsUnicode(false);

            builder.Property(e => e.IsManual)
                .HasColumnName("is_manual")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.Property(e => e.IsPosted)
                .HasColumnName("is_posted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.HasQueryFilter(r => r.Deleted == 0);
        }
    }
}
