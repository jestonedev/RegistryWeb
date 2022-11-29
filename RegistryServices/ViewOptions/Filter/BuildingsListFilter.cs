using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class BuildingsFilter : FilterAddressOptions
    {
        public int? IdBuilding { get; set; }
        public string IdRegion { get; set; }
        public string IdStreet { get; set; }
        public int? IdDecree { get; set; }
        public string House { get; set; }
        public short? Floors { get; set; }
        public short? Entrances { get; set; }
        public string CadastralNum { get; set; }
        public int? StartupYear { get; set; }
        public string RestrictionNum { get; set; }
        public DateTime? RestrictionDate { get; set; }
        public List<int> IdsRestrictionType { get; set; }
        public string NumberOwnershipRight { get; set; }
        public DateTime? DateOwnershipRight { get; set; }
        public List<int> IdsOwnershipRightType { get; set; }
        public bool? IdsOwnershipRightTypeContains { get; set; }
        public List<int> IdsObjectState { get; set; }

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
            return IsOwnershipRightEmpty() && IsRestrictionEmpty() &&
                (IdBuilding == null || IdBuilding == 0) && StartupYear == null &&
                (IdDecree == null || IdDecree == 0) && CadastralNum == null &&
                IdRegion == null && IdStreet == null && House == null && Floors == null && Entrances == null &&
                (IdsObjectState == null || IdsObjectState.Count == 0);
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }
    }
}
