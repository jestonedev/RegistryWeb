using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class OwnerVM
    {
        public Owner Owner { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}