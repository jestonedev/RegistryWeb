using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Payments;

namespace RegistryDb.Models.IEntityTypeConfiguration.Payments
{
    public class PaymentAccountCommentConfiguration : IEntityTypeConfiguration<PaymentAccountComment>
    {
        private string nameDatebase;

        public PaymentAccountCommentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PaymentAccountComment> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("payment_account_comment", nameDatebase);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.Property(e => e.Comment)
                .HasColumnName("comment")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
