using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class ObjectStates
    {
        public ObjectStates()
        {
            Buildings = new List<Buildings>();
            Premises = new List<Premises>();
            SubPremises = new List<SubPremises>();
        }

        public int IdState { get; set; }
        public string StateFemale { get; set; }
        public string StateNeutral { get; set; }

        public virtual IList<Buildings> Buildings { get; set; }
        public virtual IList<Premises> Premises { get; set; }
        public virtual IList<SubPremises> SubPremises { get; set; }
    }
}
