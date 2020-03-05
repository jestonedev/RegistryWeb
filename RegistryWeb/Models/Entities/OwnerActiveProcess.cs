using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class OwnerActiveProcess
    {
        public int IdProcess { get; set; }
        public string Owners { get; set; }
        public int CountOwners { get; set; }

        public virtual OwnerProcess OwnerProcessNavigation { get; set; }
    }
}
