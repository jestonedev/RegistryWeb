using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public class GroupChangeLog
    {
        public DateTime OperationTime { get; set; }
        public string TableName { get; set; }
        public IEnumerable<ChangeLog> Logs { get; set; }
    }
}