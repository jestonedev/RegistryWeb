using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseFacadeTypeEnum
    {
        [XmlEnum(Name = "1")]
        WallMaterial = 1,//Соответствует материалу стен

        [XmlEnum(Name = "2")]
        Plastered, //Оштукатуренный

        [XmlEnum(Name = "3")]
        Painted, //Окрашенный

        [XmlEnum(Name = "4")]
        FacedTile, //Облицованный плиткой

        [XmlEnum(Name = "5")]
        FacedStone, //Облицованный камнем

        [XmlEnum(Name = "6")]
        Siding, //Сайдинг

        [XmlEnum(Name = "7")]
        Other //Иной
    }
}