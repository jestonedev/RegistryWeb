using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseFoundationTypeEnum
    {
        [XmlEnum(Name = "1")]
        Tape = 1, //Ленточный

        [XmlEnum(Name = "2")]
        ConcretePillars, //Бетонные столбы

        [XmlEnum(Name = "3")]
        Pile, //Свайный

        [XmlEnum(Name = "4")]
        Other, //Иной

        [XmlEnum(Name = "5")]
        Columnar, //Столбчатый

        [XmlEnum(Name = "6")]
        Solid, //Сплошной

        [XmlEnum(Name = "7")]
        Prefabricated, //Сборный

        [XmlEnum(Name = "8")]
        Missing //Отсутствует
    }
}