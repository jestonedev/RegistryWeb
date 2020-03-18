using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesDoorKeys
    {
        public PremisesDoorKeys()
        {
            Premises = new List<Premise>();
        }

        public int IdPremisesDoorKeys { get; set; }
        public string LocationOfKeys { get; set; }

        public virtual IList<Premise> Premises { get; set; }
    }
}
