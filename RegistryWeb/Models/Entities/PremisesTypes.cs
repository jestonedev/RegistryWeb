using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesTypes
    {
        public PremisesTypes()
        {
            Premises = new HashSet<Premises>();
        }

        public int IdPremisesType { get; set; }
        public string PremisesType { get; set; }
        public string PremisesTypeAsNum { get; set; }
        public string PremisesTypeShort { get; set; }

        public virtual ICollection<Premises> Premises { get; set; }
    }
}
