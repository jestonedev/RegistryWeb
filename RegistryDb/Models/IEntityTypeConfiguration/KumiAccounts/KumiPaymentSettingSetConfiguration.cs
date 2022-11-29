using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentSettingSetConfiguration : IEntityTypeConfiguration<KumiPaymentSettingSet>
    {
        private string nameDatebase;

        public KumiPaymentSettingSetConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentSettingSet> builder)
        {
            builder.HasKey(e => e.IdSettingSet);

            builder.ToTable("kumi_payments_setting_sets", nameDatebase);

            builder.Property(e => e.IdSettingSet)
                .HasColumnName("id_setting_set")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.CodeUbp)
                .HasColumnName("code_ubp")
                .HasMaxLength(8)
                .IsUnicode(false);

            builder.Property(e => e.NameUbp)
                .HasColumnName("name_ubp")
                .HasMaxLength(2000)
                .IsUnicode(false);

            builder.Property(e => e.AccountUbp)
                .HasColumnName("account_ubp")
                .HasMaxLength(11)
                .IsUnicode(false);

            builder.Property(e => e.NameGrs)
                .HasColumnName("name_grs")
                .HasMaxLength(2000)
                .IsUnicode(false);

            builder.Property(e => e.GlavaGrs)
                .HasColumnName("glava_grs")
                .HasMaxLength(3)
                .IsUnicode(false);

            builder.Property(e => e.OkpoFo)
                .HasColumnName("okpo_fo")
                .HasMaxLength(8)
                .IsUnicode(false);

            builder.Property(e => e.NameFo)
                .HasColumnName("name_fo")
                .HasMaxLength(2000)
                .IsUnicode(false);

            builder.Property(e => e.AccountFo)
                .HasColumnName("account_fo")
                .HasMaxLength(11)
                .IsUnicode(false);

            builder.Property(e => e.CodeTofk)
                .HasColumnName("code_tofk")
                .HasMaxLength(4)
                .IsUnicode(false);

            builder.Property(e => e.NameTofk)
                .HasColumnName("name_tofk")
                .HasMaxLength(2000)
                .IsUnicode(false);

            builder.Property(e => e.NameBudget)
                .HasColumnName("name_budget")
                .HasMaxLength(512)
                .IsUnicode(false);

            builder.Property(e => e.BudgetLevel)
                .HasColumnName("budget_level")
                .HasMaxLength(1)
                .IsUnicode(false);
        }
    }
}
