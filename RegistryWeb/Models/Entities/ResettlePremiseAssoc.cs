using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class ResettlePremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdResettleInfo { get; set; }
        public byte Deleted { get; set; }

        public virtual ResettleInfo ResettleInfoNavigation { get; set; }
        public virtual Premise PremisesNavigation { get; set; }
    }
}
