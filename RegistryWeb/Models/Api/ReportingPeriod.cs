using System;
using System.Runtime.Serialization;

namespace RegistryWeb.Models.Api
{
    [DataContract]
    public class ReportingPeriod
    {
        [DataMember(Name= "id")]
        public int Id { get; set; }
        [DataMember(Name = "date_start")]
        public DateTime DateStart { get; set; }
        [DataMember(Name = "date_end")]
        public DateTime DateEnd { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "state")]
        public ReportingPeriodStateEnum State { get; set; }
        [DataMember(Name = "is_988")]
        public bool Is988 { get; set; }
    }
}
