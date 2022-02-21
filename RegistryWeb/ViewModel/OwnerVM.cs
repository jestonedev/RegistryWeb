using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.Enums;
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