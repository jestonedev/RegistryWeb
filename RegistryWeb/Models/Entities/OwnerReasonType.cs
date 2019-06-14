using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerReasonType
    {
        public OwnerReasonType()
        {
            OwnerReasons = new List<OwnerReason>();
        }

        public int IdReasonType { get; set; }
        public string ReasonName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<OwnerReason> OwnerReasons { get; set; }
    }
}
