using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseStageEnum
    {
        [XmlEnum(Name = "1")]
        Exploited = 1,
        [XmlEnum(Name = "2")]
        Decommissioned, //пока запретили изменение
        [XmlEnum(Name = "3")]
        Drifting //пока запретили изменение
    }
}