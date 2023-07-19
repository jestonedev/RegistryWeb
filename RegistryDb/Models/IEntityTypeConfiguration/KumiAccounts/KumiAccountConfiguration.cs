using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiAccountConfiguration : IEntityTypeConfiguration<KumiAccount>
    {
        private string nameDatebase;

        public KumiAccountConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiAccount> builder)
        {
            builder.HasKey(e => e.IdAccount);

            builder.ToTable("kumi_accounts", nameDatebase);

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Account)
                .HasColumnName("account")
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.AccountGisZkh)
                .HasColumnName("account_gis_zkh")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.CreateDate)
                .HasColumnName("create_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.AnnualDate)
                .HasColumnName("annual_date")
                .HasColumnType("date");

            builder.Property(e => e.RecalcMarker)
                .HasColumnName("recalc_marker")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.Property(e => e.RecalcReason)
                .HasColumnName("recalc_reason")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.LastChargeDate)
                .HasColumnName("last_charge_date")
                .HasColumnType("date");

            builder.Property(e => e.LastCalcDate)
                .HasColumnName("last_calc_date")
                .HasColumnType("date");

            builder.Property(e => e.CurrentBalanceTenancy)
                .HasColumnName("current_balance_tenancy")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.CurrentBalancePenalty)
                .HasColumnName("current_balance_penalty")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.CurrentBalanceDgi)
                .HasColumnName("current_balance_dgi")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.CurrentBalancePkk)
                .HasColumnName("current_balance_pkk")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.CurrentBalancePadun)
                .HasColumnName("current_balance_padun")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.Owner)
                .HasColumnName("owner")
                .HasMaxLength(355)
                .IsUnicode(false);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1024)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.HasOne(e => e.State).WithMany(e => e.KumiAccounts)
                .HasForeignKey(e => e.IdState);

            builder.HasQueryFilter(r => r.Deleted == 0);
        }
    }
}
