using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PrivEstateOwner
    {
        public PrivEstateOwner()
        {
            SelectableSigners = new List<SelectableSigner>();
        }

        public int IdOwner { get; set; }
        public string Name { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<SelectableSigner> SelectableSigners { get; set; }
    }
}
