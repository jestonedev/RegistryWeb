using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryWeb.Models.SqlViews;

namespace RegistryWeb.Models.Entities
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
        // TODO obtaining_court_order_date - claim_complete_reason
        public byte Deleted { get; set; }
        public virtual Claim IdClaimNavigation { get; set; }
        public virtual ClaimStateType IdStateTypeNavigation { get; set; }
    }
}
