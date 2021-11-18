using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PrivAgreementConfiguration : IEntityTypeConfiguration<PrivAgreement>
    {
        private string nameDatebase;

        public PrivAgreementConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivAgreement> builder)
        {
            builder.ToTable("priv_agreements", nameDatebase);

            builder.HasKey(e => e.IdAgreement);

            builder.Property(e => e.IdAgreement)
                .HasColumnName("id_agreement")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdContract)
                .HasColumnName("id_contract")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.AgreementDate)
                .HasColumnName("agreement_date")
                .HasColumnType("date");

            builder.Property(e => e.AgreementContent)
                .HasColumnName("agreement_content")
                .HasMaxLength(65535)
                .IsUnicode(false);

            builder.Property(e => e.User)
                .HasColumnName("user")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint")
                .HasDefaultValueSql("0")
                .IsRequired();

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => !e.Deleted);
        }  
    }
}
