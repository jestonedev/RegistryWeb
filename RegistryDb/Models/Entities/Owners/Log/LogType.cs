using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Owners.Log
{
    public class LogType
    {
        public LogType()
        {
            LogOwnerProcesses = new List<LogOwnerProcess>();
        }

        public int IdLogType { get; set; }
        public string Name { get; set; }

        public virtual IList<LogOwnerProcess> LogOwnerProcesses { get; set; }
    }
}
