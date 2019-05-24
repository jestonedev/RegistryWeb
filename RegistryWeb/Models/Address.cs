using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public class Address
    {
        public int IdTypeAddress { get; set; }
        public string IdStreet { get; set; }
        public int IdBuilding { get; set; }
        public int IdPremiseType { get; set; }
        public int IdPremise { get; set; }
        public int IdSubPremise { get; set; }
    }
}
