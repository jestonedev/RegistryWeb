using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class OwnerType
    {
        public OwnerType()
        {
            Owners = new List<Owner>();
        }

        public int IdOwnerType { get; set; }
        public string OwnerType1 { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<Owner> Owners { get; set; }

    }
}
