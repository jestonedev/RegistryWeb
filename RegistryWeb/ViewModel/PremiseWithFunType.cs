using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RegistryDb.Models.Entities;

namespace RegistryWeb.ViewModel
{
    public class PremiseWithFunType : Premise
    {
        public string FundType { get; set; }
    }
}
