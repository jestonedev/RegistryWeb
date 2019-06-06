using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesKinds
    {
        public PremisesKinds()
        {
            Premises = new List<Premises>();
        }

        public int IdPremisesKind { get; set; }
        public string PremisesKind { get; set; }

        public virtual IList<Premises> Premises { get; set; }
    }
}
