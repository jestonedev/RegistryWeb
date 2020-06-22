using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class ActTypeDocument
    {
        public ActTypeDocument()
        {
            PremisesJurisdictionActFiles = new List<PremisesJurisdictionActFiles>();
        }

        public int Id { get; set; }
        public string ActFileType { get; set; }
        public string Name { get; set; }

        public virtual IList<PremisesJurisdictionActFiles> PremisesJurisdictionActFiles { get; set; }
    }
}
