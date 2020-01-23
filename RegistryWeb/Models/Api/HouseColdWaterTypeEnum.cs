using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseColdWaterTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1, //отсутствует

        [XmlEnum(Name = "2")]
        Central, //центральное

        [XmlEnum(Name = "3")]
        Autonomous, //автономное
    }
}