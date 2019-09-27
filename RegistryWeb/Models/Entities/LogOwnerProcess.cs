using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
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

        public IList<LogOwnerProcessValue> GetDisplayLogsValue()
        {
            var displayLogsValue = new List<LogOwnerProcessValue>();
            if (IdLogType == 1 || IdLogType == 2 || IdLogType == 5)
                return displayLogsValue;
            return displayLogsValue;
        }
    }
}
