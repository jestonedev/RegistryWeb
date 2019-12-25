namespace RegistryWeb.Models.Api
{
    public enum HouseHotWaterTypeEnum
    {
        Missing = 1, //отсутствует
        CentralizedOpen, //централизованная открытая
        CentralizedClosed, //централизованная закрытая
        AutonomousSteamshop, //Автономная котельная (крышная, встроенно-пристроенная)
        Apartment, //Квартирное (квартирный котел)
        Stove, //Печное
        IndividualHeatPoint, //Индивидуальный тепловой пункт (ИТП)
    }
}