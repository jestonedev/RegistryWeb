using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    class KumiAccountActualTenancyProcessSearchInfoConfiguration : IEntityTypeConfiguration<KumiAccountActualTenancyProcessSearchInfo>
    {
        private string nameDatebase;

        public KumiAccountActualTenancyProcessSearchInfoConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiAccountActualTenancyProcessSearchInfo> builder)
        {
            builder.HasKey(e => e.IdRecord);

            builder.ToTable("kumi_accounts_actual_tp_search_denorm", nameDatebase);

            builder.Property(e => e.IdRecord)
                .HasColumnName("id_record")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Tenant)
                .HasColumnName("tenant")
                .HasColumnType("varchar(355)")
                .HasMaxLength(355);

            builder.Property(e => e.Prescribed)
                .HasColumnName("prescribed")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Emails)
                .HasColumnName("emails")
                .HasColumnType("varchar(2048)")
                .HasMaxLength(2048);
        }
    }
}
