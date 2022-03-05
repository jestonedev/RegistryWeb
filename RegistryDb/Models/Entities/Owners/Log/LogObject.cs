using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Owners.Log
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
