using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public partial class RentPremise
    {
        public RentPremise()
        {
            Premises = new List<Premise>();
        }

        public int IdPremises { get; set; }
        public double Payment { get; set; }

        public virtual IList<Premise> Premises { get; set; }
    }
}
