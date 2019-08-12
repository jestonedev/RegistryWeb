using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class AclUserRole
    {        
        public int IdUser { get; set; }
        public int IdRole { get; set; }

        public virtual AclUser IdAclUserNavigation { get; set; }
        public virtual AclRole IdAclRoleNavigation { get; set; }
    }
}