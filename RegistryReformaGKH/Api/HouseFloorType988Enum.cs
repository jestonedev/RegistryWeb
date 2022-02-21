using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseFloorType988Enum
    {
        [XmlEnum(Name = "1")]
        ReinforcedConcrete = 1, //Железобетонные

        [XmlEnum(Name = "2")]
        Wooden, //Деревянные

        [XmlEnum(Name = "3")]
        Mixed, //Смешанные

        [XmlEnum(Name = "4")]
        Other //Иные
    }
}
