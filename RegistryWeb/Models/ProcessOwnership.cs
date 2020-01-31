using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public class ProcessOwnership
    {
        public int Id { get; set; }
        public IEnumerable<string> Addresses { get; set; }
        public string Persons { get; set; }
        public ProcessOwnershipTypeEnum Type { get; set; }
        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
    }
}
