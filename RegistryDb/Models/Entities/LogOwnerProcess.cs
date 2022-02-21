using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class LogOwnerProcess
    {
        public LogOwnerProcess()
        {
            LogOwnerProcessesValue = new List<LogOwnerProcessValue>();
        }

        public int Id { get; set; }
        public int IdProcess { get; set; }
        public DateTime Date { get; set; }
        public int IdUser { get; set; }
        public int IdLogObject { get; set; }
        public int IdLogType { get; set; }
        public string Table { get; set; }
        public int IdKey { get; set; }

        public virtual LogObject IdLogObjectNavigation { get; set; }
        public virtual LogType IdLogTypeNavigation { get; set; }
        public virtual AclUser IdUserNavigation { get; set; }
        public virtual IList<LogOwnerProcessValue> LogOwnerProcessesValue { get; set; }
    }
}
