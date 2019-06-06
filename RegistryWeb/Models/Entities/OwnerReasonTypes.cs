using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerReasonTypes
    {
        public OwnerReasonTypes()
        {
            OwnerReasons = new List<OwnerReasons>();
        }

        public int IdReasonType { get; set; }
        public string ReasonName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<OwnerReasons> OwnerReasons { get; set; }
    }
}
