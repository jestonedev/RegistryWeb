using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class TenancyProcessesFilter : FilterAddressOptions
    {
        public int? IdProcess { get; set; }
        public string RegistrationNum { get; set; }
        public bool RegistrationNumIsEmpty { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReasonDocNum { get; set; }
        public DateTime? ReasonDocDate { get; set; }
        public List<int> IdsReasonType { get; set; }
        public string TenantSnp { get; set; }
        public string TenancyParticipantSnp { get; set; }
        public List<int> IdsRentType { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public List<int> IdsOwnershipRightType { get; set; }
        public List<int> IdsObjectState { get; set; }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }

        public bool IsModalEmpty()
        {
            return (IdProcess == null || IdProcess == 0) &&
                RegistrationNum == null && RegistrationDate == null &&
                IssuedDate == null && BeginDate == null && EndDate == null &&
                ReasonDocNum == null && ReasonDocDate == null && 
                (IdsReasonType == null || IdsReasonType.Count == 0) &&
                TenantSnp == null && TenancyParticipantSnp == null &&
                (IdsRentType == null || IdsRentType.Count == 0) &&
                IdStreet == null && House == null && PremisesNum == null &&
                (IdsOwnershipRightType == null || IdsOwnershipRightType.Count == 0) &&
                (IdsObjectState == null || IdsObjectState.Count == 0) &&
                (!RegistrationNumIsEmpty);
        }
    }
}
