using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesKinds
    {
        public PremisesKinds()
        {
            Premises = new HashSet<Premises>();
        }

        public int IdPremisesKind { get; set; }
        public string PremisesKind { get; set; }

        public virtual ICollection<Premises> Premises { get; set; }
    }
}
