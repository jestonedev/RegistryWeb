using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum ReportingPeriodStateEnum
    {
        [XmlEnum(Name = "1")]
        Current = 1,
        [XmlEnum(Name = "2")]
        Archive
    }
}
