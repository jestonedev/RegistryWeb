using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessVM
    {
        public OwnerProcesses OwnerProcess { get; set; }
        public IEnumerable<OwnerType> OwnerTypes { get; set; }
        public IList<Address> Addresses { get; set; }
        public IList<OwnerReasons> OwnerReasons { get; set; }
        public IList<OwnerPersons> OwnerPersons { get; set; }
        public IList<OwnerOrginfos> OwnerOrginfos { get; set; }
    }
}
