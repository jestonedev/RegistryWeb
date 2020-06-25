using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models
{
    public class PremiseWithFunType : Premise
    {
        public string FundType { get; set; }
    }
}
