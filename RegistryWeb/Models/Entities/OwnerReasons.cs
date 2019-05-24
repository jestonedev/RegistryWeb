using System;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerReasons
    {
        public int IdReason { get; set; }
        public int IdProcess { get; set; }
        public int IdReasonType { get; set; }
        public string ReasonNumber { get; set; }
        public DateTime? ReasonDate { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerReasonTypes IdReasonTypeNavigation { get; set; }
        public virtual OwnerProcesses IdOwnerProcessesNavigation { get; set; }
    }
}
