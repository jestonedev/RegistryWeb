using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class OwnerFileAssocVM
    {
        public OwnerFileAssoc OwnerFileAssoc { get; set; }
        public int I { get; set; }
        public int J { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}