using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class PremisesListFilter : FilterAddressOptions
    {
        //public string Street { get; set; }
        /**/
        
        public int? IdObjectState { get; set; }
        public int? IdLocationDoorKeys { get; set; }
        public int? IdComment { get; set; }
        public int? IdPremisesType { get; set; }

        public int? IdPremise { get; set; }
        //public string Region { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public short? Floors { get; set; }
        public string CadastralNum { get; set; }
        public int? IdFundType { get; set; }
        public string RestrictionNum { get; set; }
        public int? IdRestrictionType { get; set; }
        public int? NumberOwnershipPremiseType { get; set; }
        
        public string NumberOwnershipRight { get; set; }
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
                (IdFundType == null || IdFundType == 0) &&
                (IdRestrictionType == null || IdRestrictionType == 0) &&
                (IdStreet == null) && (House == null) && (Floors == null) && //(Entrances == null) &&
                (IdsObjectState == null || IdsObjectState.Count == 0) &&
                (IdsOwnershipRightType == null || IdsOwnershipRightType.Count == 0) &&
                (IdsComment == null || IdsComment.Count == 0) &&
                (IdsDoorKeys == null || IdsDoorKeys.Count == 0)
                ;
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }
    }
}
