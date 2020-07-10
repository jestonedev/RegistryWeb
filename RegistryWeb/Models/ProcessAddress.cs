using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public class ProcessAddress
    {
        public int IdProcess { get; set; }
        public int IdAssoc { get; set; }
        public int IdTypeAddress { get; set; }
        public string IdStreet { get; set; }
        public int IdBuilding { get; set; }
        public int IdPremisesType { get; set; }
        public int IdPremise { get; set; }
        public int IdSubPremise { get; set; }
        public Premise Premise { get; set; }
    }
}
