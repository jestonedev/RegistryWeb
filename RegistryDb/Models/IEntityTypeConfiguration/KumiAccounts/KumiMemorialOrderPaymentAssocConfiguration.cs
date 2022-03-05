using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiMemorialOrderPaymentAssocConfiguration : IEntityTypeConfiguration<KumiMemorialOrderPaymentAssoc>
    {
        private string nameDatebase;

        public KumiMemorialOrderPaymentAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiMemorialOrderPaymentAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("kumi_memorial_order_payment_assoc", nameDatebase);

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdOrder)
                .HasColumnName("id_order")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdPayment)
                .HasColumnName("id_payment")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.HasOne(e => e.Payment)
                .WithMany(e => e.MemorialOrderPaymentAssocs)
                .HasForeignKey(e => e.IdPayment);

            builder.HasOne(e => e.Order)
                .WithMany(e => e.MemorialOrderPaymentAssocs)
                .HasForeignKey(e => e.IdOrder);
        }
    }
}
