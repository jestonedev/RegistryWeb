using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class PrivContractorVM
    {
        public PrivContractor PrivContractor { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}