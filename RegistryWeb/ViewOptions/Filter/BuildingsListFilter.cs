using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class BuildingsFilter : FilterAddressOptions
    {
        public int? IdBuilding { get; set; }
        //public string Region { get; set; }
        public string IdStreet { get; set; }
        public int? IdDecree { get; set; }
        public string House { get; set; }
        public short? Floors { get; set; }
        public short? Entrances { get; set; }
        public string NumberOwnershipRight { get; set; }
        public DateTime? DateOwnershipRight { get; set; }
        public List<int> IdsOwnershipRightType { get; set; }
        public List<int> IdsObjectState { get; set; }

        public bool IsOwnershipRightEmpty()
        {
            return (NumberOwnershipRight == null) && (DateOwnershipRight == null) &&
                (IdsOwnershipRightType == null || IdsOwnershipRightType.Count == 0);
        }

        public bool IsModalEmpty()
        {
            return IsOwnershipRightEmpty() &&
                (IdBuilding == null || IdBuilding == 0) &&
                (IdDecree == null || IdDecree == 0) &&
                (IdStreet == null) && (House == null) && (Floors == null) && (Entrances == null) &&
                (IdsObjectState == null || IdsObjectState.Count == 0);
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }
    }
}
