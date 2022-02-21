using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public class PrivTypeOfProperty
    {
        public int IdTypeOfProperty { get; set; }
        public string Name { get; set; }
        public virtual IList<PrivContract> PrivContracts { get; set; }
    }
}