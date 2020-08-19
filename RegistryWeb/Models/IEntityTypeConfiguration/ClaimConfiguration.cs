using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
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

            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
