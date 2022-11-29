using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPayerStatusConfiguration : IEntityTypeConfiguration<KumiPayerStatus>
    {
        private string nameDatebase;

        public KumiPayerStatusConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPayerStatus> builder)
        {
            builder.HasKey(e => e.IdPayerStatus);

            builder.ToTable("kumi_payer_status", nameDatebase);

            builder.Property(e => e.IdPayerStatus)
                .HasColumnName("id_payer_status")
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
