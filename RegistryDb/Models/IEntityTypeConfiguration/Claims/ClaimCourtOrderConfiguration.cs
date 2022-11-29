using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;

namespace RegistryDb.Models.IEntityTypeConfiguration.Claims
{
    public class ClaimCourtOrderConfiguration : IEntityTypeConfiguration<ClaimCourtOrder>
    {
        private string nameDatebase;

        public ClaimCourtOrderConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ClaimCourtOrder> builder)
        {
            builder.HasKey(e => e.IdOrder);

            builder.ToTable("claim_court_orders", nameDatebase);

            builder.Property(e => e.IdOrder)
                .HasColumnName("id_order")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdExecutor)
                .HasColumnName("id_executor")
                .HasColumnType("int(11)");

            builder.Property(e => e.CreateDate)
                .HasColumnName("create_date")
                .HasColumnType("date");

            builder.Property(e => e.IdSigner)
                .HasColumnName("id_signer")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdJudge)
                .HasColumnName("id_judge")
                .HasColumnType("int(11)");

            builder.Property(e => e.OrderDate)
                .HasColumnName("order_date")
                .HasColumnType("date");

            builder.Property(e => e.OpenAccountDate)
                .HasColumnName("open_account_date")
                .HasColumnType("date");

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

            builder.Property(e => e.StartDeptPeriod)
                .HasColumnName("start_dept_period")
                .HasColumnType("date");

            builder.Property(e => e.EndDeptPeriod)
               .HasColumnName("end_dept_period")
               .HasColumnType("date");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(e => e.IdClaimNavigation)
                .WithMany(e => e.ClaimCourtOrders)
                .HasForeignKey(d => d.IdClaim)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claim_court_orders_id_claim");

            builder.HasOne(e => e.IdExecutorNavigation)
                .WithMany(e => e.ClaimCourtOrders)
                .HasForeignKey(d => d.IdExecutor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claim_court_orders_id_executor");

            builder.HasOne(e => e.IdJudgeNavigation)
                .WithMany(e => e.ClaimCourtOrders)
                .HasForeignKey(d => d.IdJudge)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claim_court_orders_id_judge");

            builder.HasOne(e => e.IdSignerNavigation)
                .WithMany(e => e.ClaimCourtOrders)
                .HasForeignKey(d => d.IdSigner)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claim_court_orders_id_signer");

            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
