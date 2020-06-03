using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class TenancyReasonType
    {
        public TenancyReasonType()
        {
            TenancyReasons = new List<TenancyReason>();
        }

        public int IdReasonType { get; set; }
        public string ReasonName { get; set; }
        public string ReasonTemplate { get; set; }
        public int Order { get; set; }
        public int Deleted { get; set; }
        public virtual IList<TenancyReason> TenancyReasons { get; set; }
    }
}
