using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class ObjectStates
    {
        public ObjectStates()
        {
            Buildings = new HashSet<Buildings>();
            Premises = new HashSet<Premises>();
            SubPremises = new HashSet<SubPremises>();
        }

        public int IdState { get; set; }
        public string StateFemale { get; set; }
        public string StateNeutral { get; set; }

        public virtual ICollection<Buildings> Buildings { get; set; }
        public virtual ICollection<Premises> Premises { get; set; }
        public virtual ICollection<SubPremises> SubPremises { get; set; }
    }
}
