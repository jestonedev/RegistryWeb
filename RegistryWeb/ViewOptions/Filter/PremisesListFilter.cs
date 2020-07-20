using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class PremisesListFilter : FilterAddressOptions
    {
        public int? IdPremise { get; set; }
        public int? IdBuilding { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public short? Floors { get; set; }
        public string CadastralNum { get; set; }
        public List<int> IdFundType { get; set; }
        public string RestrictionNum { get; set; }
        public DateTime? RestrictionDate { get; set; }
        public List<int> IdsRestrictionType { get; set; }
        public string NumberOwnershipRight { get; set; }
        public DateTime? DateOwnershipRight { get; set; }
        public DateTime? StDateOwnershipRight { get; set; }
        public DateTime? EndDateOwnershipRight { get; set; }
        public List<int> IdsOwnershipRightType { get; set; }
        public List<int> IdsObjectState { get; set; }
        public List<int> IdsComment { get; set; }
        public List<int> IdsDoorKeys { get; set; }

        public bool IsModalEmpty()
        {
            return
                (IdPremise == null || IdPremise == 0) &&
                IdStreet == null && House == null && PremisesNum == null &&
                (IdFundType == null || IdFundType.Count == 0) &&
                (IdsRestrictionType == null || IdsRestrictionType.Count == 0) &&
                RestrictionNum == null && RestrictionDate == null &&
                (Floors == null) && CadastralNum == null &&
                (IdsObjectState == null || IdsObjectState.Count == 0) &&
                (IdsOwnershipRightType == null || IdsOwnershipRightType.Count == 0) &&
                NumberOwnershipRight == null && DateOwnershipRight == null &&
                (IdsComment == null || IdsComment.Count == 0) &&
                (IdsDoorKeys == null || IdsDoorKeys.Count == 0) &&
                StDateOwnershipRight == null && EndDateOwnershipRight == null;
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }
    }
}
