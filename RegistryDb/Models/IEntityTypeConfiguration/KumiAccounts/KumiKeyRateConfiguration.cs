using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiKeyRateConfiguration : IEntityTypeConfiguration<KumiKeyRate>
    {
        private string nameDatebase;

        public KumiKeyRateConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiKeyRate> builder)
        {
            builder.HasKey(e => e.IdRecord);

            builder.ToTable("kumi_key_rates", nameDatebase);

            builder.Property(e => e.IdRecord)
                .HasColumnName("id_record")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.Value)
                .HasColumnName("value")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
        }
    }
}
