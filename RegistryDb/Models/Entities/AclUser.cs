﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.Entities
{
    public class AclUser
    {
        public AclUser()
        {
            AclUserPrivileges = new List<AclUserPrivilege>();
            AclUserRoles = new List<AclUserRole>();
            LogOwnerProcesses = new List<LogOwnerProcess>();
        }

        public int IdUser { get; set; }
        public string UserName { get; set; }
        public string UserDescription { get; set; }

        public virtual PersonalSetting PersonalSetting { get; set; }
        public virtual IList<AclUserPrivilege> AclUserPrivileges { get; set; }
        public virtual IList<AclUserRole> AclUserRoles { get; set; }
        public virtual IList<LogOwnerProcess> LogOwnerProcesses { get; set; }
    }
}
