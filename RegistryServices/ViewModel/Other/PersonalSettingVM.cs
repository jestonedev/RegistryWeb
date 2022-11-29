﻿using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Acl;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Other
{
    public class PersonalSettingVM
    {
        public AclUser User { get; set; }
        public List<AclPrivilege> Privileges { get; set; }
        public string Database { get; set; }
    }
}
