using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class OwnerFileAssocVM
    {
        public OwnerFileAssoc OwnerFileAssoc { get; set; }
        public IList<OwnerFile> OwnerFiles { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}