using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class LogOwnerProcessValue
    {
        public int Id { get; set; }
        public int IdLog { get; set; }
        
        public string Field { get; set; }
        public string Value { get; set; }

        public virtual LogOwnerProcess IdLogNavigation { get; set; }
    }
}
