using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentGroupLogConfiguration : IEntityTypeConfiguration<KumiPaymentGroupLog>
    {
        private string nameDatebase;

        public KumiPaymentGroupLogConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentGroupLog> builder)
        {
            builder.HasKey(e => e.IdLog);

            builder.ToTable("kumi_payment_groups_log", nameDatebase);

            builder.Property(e => e.IdLog)
                .HasColumnName("id_log")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdGroup)
                .HasColumnName("id_group")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Log)
                .HasColumnName("log")
                .IsUnicode(false);
        }
    }
}
