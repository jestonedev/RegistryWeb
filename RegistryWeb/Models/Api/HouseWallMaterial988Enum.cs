using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseWallMaterial988Enum
    {
        [XmlEnum(Name = "1")]
        Stone = 1, //Каменные, кирпичные

        [XmlEnum(Name = "2")]
        Panel, //Панельные

        [XmlEnum(Name = "3")]
        Blocky, //Блочные

        [XmlEnum(Name = "4")]
        Mixed, //Смешанные

        [XmlEnum(Name = "5")]
        Wooden, //Деревянные

        [XmlEnum(Name = "6")]
        Monolithic, //Монолитные

        [XmlEnum(Name = "7")]
        Other, //Иные

        [XmlEnum(Name = "8")]
        NotFilled, //Не заполнено

        [XmlEnum(Name = "9")]
        ClayditeConcreteBlocks, //Керамзитобетон (блоки)

        [XmlEnum(Name = "10")]
        FerroConcrete, //Железобетон

        [XmlEnum(Name = "11")]
        ClayditeConcrete, //Керамзитобетон

        [XmlEnum(Name = "12")]
        FerroConcretePanel, //Железобетонная панель

        [XmlEnum(Name = "13")]
        ClayditeConcreteOneLayerPanel, //Керамзитобетонная 1-слойная панель

        [XmlEnum(Name = "14")]
        FerroConcreteThreeLayerPanel, //Ж/б 3-х слойная панель с утеплителем

        [XmlEnum(Name = "15")]
        SlagConcreteBlocks, //Шлакобетон (блоки)

        [XmlEnum(Name = "16")]
        SlagClayditeConcreteOneLayerPanel //Шлакокерамзитобетонная 1-слойная панель
    }
}
