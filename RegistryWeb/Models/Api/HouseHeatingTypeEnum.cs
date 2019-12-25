namespace RegistryWeb.Models.Api
{
    public enum HouseHeatingTypeEnum
    {
        Missing = 1, //Отсутствует
        Central, //Центральное
        AutonomousSteamshop, //Автономная котельная (крышная, встроенно-пристроенная)
        ApartmentHeating, //Квартирное отопление (квартирный котел)
        Stove, //Печное
        ElectricHeating, //Электроотопление
        IndividualHeatPoint, //Индивидуальный тепловой пункт (ИТП)
        Geyser //Газовая колонка
    }
}