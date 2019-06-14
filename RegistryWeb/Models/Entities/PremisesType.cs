using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesType
    {
        public PremisesType()
        {
            Premises = new List<Premise>();
        }

        public int IdPremisesType { get; set; }
        public string PremisesTypeName { get; set; }
        public string PremisesTypeAsNum { get; set; }
        public string PremisesTypeShort { get; set; }

        public virtual IList<Premise> Premises { get; set; }
    }
}
