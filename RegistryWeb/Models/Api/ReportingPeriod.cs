using System;
namespace RegistryWeb.Models.Api
{
    public class ReportingPeriod
    {
        public int Id { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string Name { get; set; }
        public ReportingPeriodStateEnum State { get; set; }
        public bool Is988 { get; set; }
    }
}
