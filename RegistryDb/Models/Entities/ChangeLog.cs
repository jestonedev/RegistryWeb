using System;

namespace RegistryDb.Models.Entities
{
    public class ChangeLog
    {
        public int IdRecord { get; set; }
        public string TableName { get; set; }
        public int IdKey { get; set; }
        public string FieldName { get; set; }
        public string FieldOldValue { get; set; }
        public string FieldNewValue { get; set; }
        public string OperationType { get; set; }
        public DateTime OperationTime { get; set; }
        public string UserName { get; set; }
    }
}