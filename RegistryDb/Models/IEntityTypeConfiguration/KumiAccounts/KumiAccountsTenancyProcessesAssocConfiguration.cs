using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiAccountsTenancyProcessesAssocConfiguration : IEntityTypeConfiguration<KumiAccountsTenancyProcessesAssoc>
    {
        private string nameDatebase;

        public KumiAccountsTenancyProcessesAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiAccountsTenancyProcessesAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("kumi_accounts_t_processes_assoc", nameDatebase);

            builder.HasIndex(e => e.IdAccount)
                .HasName("FK_kumi_accounts_t_processes_a");

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_kumi_accounts_t_processes_2");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.Fraction)
                .HasColumnName("fraction")
                .HasColumnType("decimal(10,6)");

            builder.HasOne(d => d.AccountNavigation)
                .WithMany(p => p.AccountsTenancyProcessesAssoc)
                .HasForeignKey(d => d.IdAccount);

            builder.HasOne(d => d.ProcessNavigation)
                .WithMany(p => p.AccountsTenancyProcessesAssoc)
                .HasForeignKey(d => d.IdProcess);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
