using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
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

            builder.Property(e => e.BksRequester)
                .HasColumnName("bks_requester")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.TransferToLegalDepartmentDate)
                .HasColumnName("transfert_to_legal_department_date")
                .HasColumnType("date");

            builder.Property(e => e.TransferToLegalDepartmentWho)
                .HasColumnName("transfer_to_legal_department_who")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.AcceptedByLegalDepartmentDate)
                .HasColumnName("accepted_by_legal_department_date")
                .HasColumnType("date");

            builder.Property(e => e.AcceptedByLegalDepartmentWho)
                .HasColumnName("accepted_by_legal_department_who")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ClaimDirectionDate)
                .HasColumnName("claim_direction_date")
                .HasColumnType("date");

            builder.Property(e => e.ClaimDirectionDescription)
                .HasColumnName("claim_direction_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.CourtOrderDate)
                .HasColumnName("court_order_date")
                .HasColumnType("date");

            builder.Property(e => e.CourtOrderNum)
                .HasColumnName("court_order_num")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ObtainingCourtOrderDate)
                .HasColumnName("obtaining_court_order_date")
                .HasColumnType("date");

            builder.Property(e => e.ObtainingCourtOrderDescription)
                .HasColumnName("obtaining_court_order_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.DirectionCourtOrderBailiffsDate)
                .HasColumnName("direction_court_order_bailiffs_date")
                .HasColumnType("date");

            builder.Property(e => e.DirectionCourtOrderBailiffsDescription)
                .HasColumnName("direction_court_order_bailiffs_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.EnforcementProceedingStartDate)
                .HasColumnName("enforcement_proceeding_start_date")
                .HasColumnType("date");

            builder.Property(e => e.EnforcementProceedingStartDescription)
                .HasColumnName("enforcement_proceeding_start_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.EnforcementProceedingEndDate)
                .HasColumnName("enforcement_proceeding_end_date")
                .HasColumnType("date");

            builder.Property(e => e.EnforcementProceedingEndDescription)
                .HasColumnName("enforcement_proceeding_end_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.EnforcementProceedingTerminateDate)
                .HasColumnName("enforcement_proceeding_terminate_date")
                .HasColumnType("date");

            builder.Property(e => e.EnforcementProceedingTerminateDescription)
                .HasColumnName("enforcement_proceeding_terminate_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.RepeatedDirectionCourtOrderBailiffsDate)
                .HasColumnName("repeated_direction_court_order_bailiffs_date")
                .HasColumnType("date");

            builder.Property(e => e.RepeatedDirectionCourtOrderBailiffsDescription)
                .HasColumnName("repeated_direction_court_order_bailiffs_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.RepeatedEnforcementProceedingStartDate)
                .HasColumnName("repeated_enforcement_proceeding_start_date")
                .HasColumnType("date");

            builder.Property(e => e.RepeatedEnforcementProceedingStartDescription)
                .HasColumnName("repeated_enforcement_proceeding_start_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.RepeatedEnforcementProceedingEndDate)
                .HasColumnName("repeated_enforcement_proceeding_end_date")
                .HasColumnType("date");

            builder.Property(e => e.RepeatedEnforcementProceedingEndDescription)
                .HasColumnName("repeated_enforcement_proceeding_end_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.CourtOrderCancelDate)
                .HasColumnName("court_order_cancel_date")
                .HasColumnType("date");

            builder.Property(e => e.CourtOrderCancelDescription)
                .HasColumnName("court_order_cancel_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.CourtOrderCompleteDate)
            .HasColumnName("claim_complete_date")
            .HasColumnType("date");

            builder.Property(e => e.CourtOrderCompleteDescription)
                .HasColumnName("claim_complete_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.CourtOrderCompleteReason)
                .HasColumnName("claim_complete_reason")
                .HasMaxLength(255)
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
