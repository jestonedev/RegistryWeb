using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public class PrivTypeOfProperty
    {
        public int IdTypeOfProperty { get; set; }
        public string Name { get; set; }
        public virtual IList<PrivContract> PrivContracts { get; set; }
    }
}