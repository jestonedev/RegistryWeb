using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerType
    {
        public OwnerType()
        {
            OwnerProcesses = new HashSet<OwnerProcesses>();
        }

        public int IdOwnerType { get; set; }
        public string OwnerType1 { get; set; }
        public byte Deleted { get; set; }

        public virtual ICollection<OwnerProcesses> OwnerProcesses { get; set; }
    }
}
