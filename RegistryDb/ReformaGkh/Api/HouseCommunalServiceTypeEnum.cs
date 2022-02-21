using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseCommunalServiceTypeEnum
    {
        [XmlEnum(Name = "1")]
        ColdWater = 1, //Холодное водоснабжение

        [XmlEnum(Name = "2")]
        HotWater, //Горячее водоснабжение

        [XmlEnum(Name = "3")]
        WaterDisposal, //Водоотведение

        [XmlEnum(Name = "4")]
        ElectroSupply, //Электроснабжение

        [XmlEnum(Name = "5")]
        Heating, //Отопление

        [XmlEnum(Name = "6")]
        GasSupply, //Газоснабжение

        [XmlEnum(Name = "7")]
        ColdWaterGvs, //Холодная вода для нужд ГВС

        [XmlEnum(Name = "8")]
        HeatEnergyGvs, //Тепловая энергия для подогрева холодной воды для нужд ГВС

        [XmlEnum(Name = "9")]
        GasSupplyGvs, //Газоснабжение для подогрева холодной воды для нужд ГВС

        [XmlEnum(Name = "10")]
        ComponentHeatEnergyGvs, //Компонент на тепловую энергию для ГВС

        [XmlEnum(Name = "11")]
        SolidWaste, //Обращение с твердыми коммунальными отходами

        [XmlEnum(Name = "12")]
        ComponentHeatcarrierGvs //Компонент на теплоноситель для ГВС
    }
}