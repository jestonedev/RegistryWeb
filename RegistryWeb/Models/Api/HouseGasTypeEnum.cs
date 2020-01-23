using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseGasTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1, //отсутствует

        [XmlEnum(Name = "2")]
        Central, //центральное

        [XmlEnum(Name = "3")]
        Autonomous, //автономное
    }
}