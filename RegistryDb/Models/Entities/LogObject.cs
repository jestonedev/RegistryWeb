using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.Entities
{
    public class LogObject
    {
        public LogObject()
        {
            LogOwnerProcesses = new List<LogOwnerProcess>();
        }

        public int IdLogObject { get; set; }
        public string Name { get; set; }

        public virtual IList<LogOwnerProcess> LogOwnerProcesses { get; set; }
    }
}
