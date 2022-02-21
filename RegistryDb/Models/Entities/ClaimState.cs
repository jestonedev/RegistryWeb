﻿using System;

namespace RegistryDb.Models.Entities
{
    public class ClaimState
    {
        public int IdState { get; set; }
        public int IdClaim { get; set; }
        public int IdStateType { get; set; }
        public DateTime? DateStartState { get; set; }
        public string Executor { get; set; }
        public string Description { get; set; }
        public string BksRequester { get; set; }
        public DateTime? TransferToLegalDepartmentDate { get; set; }
        public string TransferToLegalDepartmentWho { get; set; }
        public DateTime? AcceptedByLegalDepartmentDate { get; set; }
        public string AcceptedByLegalDepartmentWho { get; set; }
        public DateTime? ClaimDirectionDate { get; set; }
        public string ClaimDirectionDescription { get; set; }
        public DateTime? CourtOrderDate { get; set; }
        public string CourtOrderNum { get; set; }
        public DateTime? ObtainingCourtOrderDate { get; set; }
        public string ObtainingCourtOrderDescription { get; set; }
        public DateTime? DirectionCourtOrderBailiffsDate { get; set; }
        public string DirectionCourtOrderBailiffsDescription { get; set; }
        public DateTime? EnforcementProceedingStartDate { get; set; }
        public string EnforcementProceedingStartDescription { get; set; }
        public DateTime? EnforcementProceedingEndDate { get; set; }
        public string EnforcementProceedingEndDescription { get; set; }
        public DateTime? EnforcementProceedingTerminateDate { get; set; }
        public string EnforcementProceedingTerminateDescription { get; set; }
        public DateTime? RepeatedDirectionCourtOrderBailiffsDate { get; set; }
        public string RepeatedDirectionCourtOrderBailiffsDescription { get; set; }
        public DateTime? RepeatedEnforcementProceedingStartDate { get; set; }
        public string RepeatedEnforcementProceedingStartDescription { get; set; }
        public DateTime? RepeatedEnforcementProceedingEndDate { get; set; }
        public string RepeatedEnforcementProceedingEndDescription { get; set; }
        public DateTime? CourtOrderCancelDate { get; set; }
        public string CourtOrderCancelDescription { get; set; }
        public DateTime? CourtOrderCompleteDate { get; set; }
        public string CourtOrderCompleteDescription { get; set; }
        public string CourtOrderCompleteReason { get; set; }
        public byte Deleted { get; set; }
        public virtual Claim IdClaimNavigation { get; set; }
        public virtual ClaimStateType IdStateTypeNavigation { get; set; }
    }
}
