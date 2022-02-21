using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class RestrictionType
    {
        public RestrictionType()
        {
            Restrictions = new List<Restriction>();
        }

        public int IdRestrictionType { get; set; }
        public string RestrictionTypeName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<Restriction> Restrictions { get; set; }
    }
}
