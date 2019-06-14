using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerType
    {
        public OwnerType()
        {
            OwnerProcesses = new List<OwnerProcess>();
        }

        public int IdOwnerType { get; set; }
        public string OwnerType1 { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<OwnerProcess> OwnerProcesses { get; set; }
    }
}
