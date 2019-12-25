namespace RegistryWeb.Models.Api
{
    public enum HouseCommunalServiceTypeEnum
    {
        ColdWater = 1, //Холодное водоснабжение
        HotWater, //Горячее водоснабжение
        WaterDisposal, //Водоотведение
        ElectroSupply, //Электроснабжение
        Heating, //Отопление
        GasSupply, //Газоснабжение
        ColdWaterGvs, //Холодная вода для нужд ГВС
        HeatEnergyGvs, //Тепловая энергия для подогрева холодной воды для нужд ГВС
        GasSupplyGvs, //Газоснабжение для подогрева холодной воды для нужд ГВС
        ComponentHeatEnergyGvs, //Компонент на тепловую энергию для ГВС
        SolidWaste, //Обращение с твердыми коммунальными отходами
        ComponentHeatcarrierGvs //Компонент на теплоноситель для ГВС
    }
}