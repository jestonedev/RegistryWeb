using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseHotWaterTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1, //отсутствует

        [XmlEnum(Name = "2")]
        CentralizedOpen, //централизованная открытая

        [XmlEnum(Name = "3")]
        CentralizedClosed, //централизованная закрытая

        [XmlEnum(Name = "4")]
        AutonomousSteamshop, //Автономная котельная (крышная, встроенно-пристроенная)

        [XmlEnum(Name = "5")]
        Apartment, //Квартирное (квартирный котел)

        [XmlEnum(Name = "6")]
        Stove, //Печное

        [XmlEnum(Name = "7")]
        IndividualHeatPoint, //Индивидуальный тепловой пункт (ИТП)
    }
}