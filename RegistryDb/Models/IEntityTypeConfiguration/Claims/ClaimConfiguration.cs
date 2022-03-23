using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities.Claims;

namespace RegistryDb.Models.IEntityTypeConfiguration.Claims
{
    public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
    {
        private string nameDatebase;

        public ClaimConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Claim> builder)
        {
            builder.HasKey(e => e.IdClaim);

            builder.ToTable("claims", nameDatebase);

            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdAccountAdditional)
                 .HasColumnName("id_account_additional")
                 .HasColumnType("int(11)");

            builder.Property(e => e.IdAccountKumi)
                 .HasColumnName("id_account_kumi")
                 .HasColumnType("int(11)");

            builder.Property(e => e.AmountTenancy)
                .HasColumnName("amount_tenancy")
                .HasColumnType("decimal");

            builder.Property(e => e.AmountPenalties)
                .HasColumnName("amount_penalties")
                .HasColumnType("decimal");

            builder.Property(e => e.AmountDgi)
                .HasColumnName("amount_dgi")
                .HasColumnType("decimal");

            builder.Property(e => e.AmountPadun)
                .HasColumnName("amount_padun")
                .HasColumnType("decimal");

            builder.Property(e => e.AmountPkk)
                .HasColumnName("amount_pkk")
                .HasColumnType("decimal");

            builder.Property(e => e.AmountTenancyRecovered)
                .HasColumnName("amount_tenancy_recovered")
                .HasColumnType("decimal");

            builder.Property(e => e.AmountPenaltiesRecovered)
                .HasColumnName("amount_penalties_recovered")
                .HasColumnType("decimal");

            builder.Property(e => e.AtDate)
                .HasColumnName("at_date")
                .HasColumnType("date");

            builder.Property(e => e.StartDeptPeriod)
                .HasColumnName("start_dept_period")
                .HasColumnType("date");

            builder.Property(e => e.EndDeptPeriod)
               .HasColumnName("end_dept_period")
               .HasColumnType("date");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsUnicode(false);

            builder.Property(e => e.EndedForFilter)
                .HasColumnName("ended_for_filter")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.LastAccountBalanceOutput)
                .HasColumnName("last_account_balance_output")
                .HasColumnType("decimal");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(e => e.IdAccountNavigation)
                .WithMany(e => e.Claims)
                .HasForeignKey(d => d.IdAccount)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claims_payments_accounts_id_account");

            builder.HasOne(e => e.IdAccountAdditionalNavigation)
                .WithMany(e => e.ClaimsByAdditional)
                .HasForeignKey(d => d.IdAccountAdditional)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claims_id_account_additional");

            builder.HasOne(e => e.IdAccountKumiNavigation)
               .WithMany(e => e.Claims)
               .HasForeignKey(d => d.IdAccountKumi);

            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
