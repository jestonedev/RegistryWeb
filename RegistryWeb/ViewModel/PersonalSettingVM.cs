using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class PersonalSettingVM
    {
        public AclUser User { get; set; }
        public List<AclPrivilege> Privileges { get; set; }
        public string Database { get; set; }
    }
}
