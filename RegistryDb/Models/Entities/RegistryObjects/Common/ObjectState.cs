using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Common
{
    public partial class ObjectState
    {
        public ObjectState()
        {
            Buildings = new List<Building>();
            Premises = new List<Premise>();
            SubPremises = new List<SubPremise>();
        }

        public int IdState { get; set; }
        public string StateFemale { get; set; }
        public string StateNeutral { get; set; }

        public virtual IList<Building> Buildings { get; set; }
        public virtual IList<Premise> Premises { get; set; }
        public virtual IList<SubPremise> SubPremises { get; set; }
    }
}
