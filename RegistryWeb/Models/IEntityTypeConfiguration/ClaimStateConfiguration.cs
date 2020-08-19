using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ClaimStateConfiguration : IEntityTypeConfiguration<ClaimState>
    {
        private string nameDatebase;

        public ClaimStateConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ClaimState> builder)
        {
            builder.HasKey(e => e.IdState);

            builder.ToTable("claim_states", nameDatebase);

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdStateType)
                .HasColumnName("id_state_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.DateStartState)
                .HasColumnName("date_start_state")
                .HasColumnType("date");

            builder.Property(e => e.Executor)
                .HasColumnName("executor")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(e => e.IdClaimNavigation)
                .WithMany(e => e.ClaimStates)
                .HasForeignKey(d => d.IdClaim)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claim_states_claims_id_claim");

            builder.HasOne(e => e.IdStateTypeNavigation)
                .WithMany(e => e.ClaimStates)
                .HasForeignKey(d => d.IdStateType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claim_states_state_types_id_state_type");

            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
