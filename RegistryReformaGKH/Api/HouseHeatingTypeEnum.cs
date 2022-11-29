using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseHeatingTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1, //Отсутствует

        [XmlEnum(Name = "2")]
        Central, //Центральное

        [XmlEnum(Name = "3")]
        AutonomousSteamshop, //Автономная котельная (крышная, встроенно-пристроенная)

        [XmlEnum(Name = "4")]
        ApartmentHeating, //Квартирное отопление (квартирный котел)

        [XmlEnum(Name = "5")]
        Stove, //Печное

        [XmlEnum(Name = "6")]
        ElectricHeating, //Электроотопление

        [XmlEnum(Name = "7")]
        IndividualHeatPoint, //Индивидуальный тепловой пункт (ИТП)

        [XmlEnum(Name = "8")]
        Geyser //Газовая колонка
    }
}