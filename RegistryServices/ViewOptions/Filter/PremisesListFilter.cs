using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class PremisesListFilter : FilterAddressOptions
    {
        public int? IdPremise { get; set; }
        public int? IdBuilding { get; set; }
        public string IdRegion { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public short? Floors { get; set; }
        public string CadastralNum { get; set; }
        public List<int> IdFundType { get; set; }
        public string RestrictionNum { get; set; }
        public DateTime? RestrictionDate { get; set; }
        public List<int> IdsRestrictionType { get; set; }
        public bool? IdsRestrictionTypeContains { get; set; }
        public string NumberOwnershipRight { get; set; }
        public DateTime? DateOwnershipRight { get; set; }
        public DateTime? StDateOwnershipRight { get; set; }
        public DateTime? EndDateOwnershipRight { get; set; }
        public List<int> IdsOwnershipRightType { get; set; }
        public bool? IdsOwnershipRightTypeContains { get; set; }
        public List<int> IdsObjectState { get; set; }
        public List<int> IdsComment { get; set; }
        public bool? IdsCommentContains { get; set; }
        public List<int> IdsDoorKeys { get; set; }
        public bool? IdsDoorKeysContains { get; set; }

        public bool IsRestrictionEmpty()
        {
            return (IdsRestrictionType == null || IdsRestrictionType.Count == 0) &&
                RestrictionNum == null && RestrictionDate == null;
        }

        public bool IsOwnershipRightEmpty()
        {
            return (NumberOwnershipRight == null) && (DateOwnershipRight == null) &&
                (IdsOwnershipRightType == null || IdsOwnershipRightType.Count == 0);
        }

        public bool IsModalEmpty()
        {
            return
                IsOwnershipRightEmpty() && IsRestrictionEmpty() &&
                (IdPremise == null || IdPremise == 0) &&
                IdRegion == null && IdStreet == null && House == null && PremisesNum == null &&
                (IdFundType == null || IdFundType.Count == 0) &&
                (Floors == null) && CadastralNum == null &&
                (IdsObjectState == null || IdsObjectState.Count == 0) &&
                (IdsComment == null || IdsComment.Count == 0) &&
                (IdsDoorKeys == null || IdsDoorKeys.Count == 0) &&
                StDateOwnershipRight == null && EndDateOwnershipRight == null;
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty() && IdBuilding == null;
        }
    }
}
