using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PaymentAccountSubPremiseAssocConfiguration : IEntityTypeConfiguration<PaymentAccountSubPremiseAssoc>
    {
        private string nameDatebase;

        public PaymentAccountSubPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PaymentAccountSubPremiseAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("payments_account_sub_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdSubPremise)
                .HasName("FK_payments_account_sub_premises_assoc_id_sub_premises");

            builder.HasIndex(e => e.IdAccount)
                .HasName("FK_payments_account_sub_premises_assoc_payments");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSubPremise)
                .HasColumnName("id_sub_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.SubPremiseNavigation)
                .WithMany(p => p.PaymentAccountSubPremisesAssoc)
                .HasForeignKey(d => d.IdSubPremise);

            builder.HasOne(d => d.PaymentAccountNavigation)
                .WithMany(p => p.PaymentAccountSubPremisesAssoc)
                .HasForeignKey(d => d.IdAccount);
        }
    }
}
