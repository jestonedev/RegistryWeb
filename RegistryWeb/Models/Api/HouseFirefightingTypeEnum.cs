using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseFirefightingTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1, //отсутствует

        [XmlEnum(Name = "2")]
        Automatic, //Автоматическая

        [XmlEnum(Name = "3")]
        FireHydrant, //Пожарные гидранты

        [XmlEnum(Name = "4")]
        FireCrane //Пожарный кран
    }
}