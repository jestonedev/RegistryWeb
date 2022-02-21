using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PaymentAccountPremiseAssocConfiguration : IEntityTypeConfiguration<PaymentAccountPremiseAssoc>
    {
        private string nameDatebase;

        public PaymentAccountPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PaymentAccountPremiseAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("payments_account_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdPremise)
                .HasName("FK_payments_account_premises_assoc_id_premises");

            builder.HasIndex(e => e.IdAccount)
                .HasName("FK_payments_account_premises_assoc_payments_accounts_id_account");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPremise)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.PremiseNavigation)
                .WithMany(p => p.PaymentAccountPremisesAssoc)
                .HasForeignKey(d => d.IdPremise);

            builder.HasOne(d => d.PaymentAccountNavigation)
                .WithMany(p => p.PaymentAccountPremisesAssoc)
                .HasForeignKey(d => d.IdAccount);
        }
    }
}
