using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class OwnerVM
    {
        public Owner Owner { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}
