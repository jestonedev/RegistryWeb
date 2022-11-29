using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Premises
{
    public partial class PremisesKind
    {
        public PremisesKind()
        {
            Premises = new List<Premise>();
        }

        public int IdPremisesKind { get; set; }
        public string PremisesKindName { get; set; }

        public virtual IList<Premise> Premises { get; set; }
    }
}
