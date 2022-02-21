using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class LitigationPremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdLitigation { get; set; }
        public byte Deleted { get; set; }

        public virtual Litigation LitigationNavigation { get; set; }
        public virtual Premise PremiseNavigation { get; set; }
    }
}
