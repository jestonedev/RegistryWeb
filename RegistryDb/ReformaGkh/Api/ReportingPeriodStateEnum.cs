using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum ReportingPeriodStateEnum
    {
        [XmlEnum(Name = "1")]
        Current = 1,
        [XmlEnum(Name = "2")]
        Archive
    }
}
