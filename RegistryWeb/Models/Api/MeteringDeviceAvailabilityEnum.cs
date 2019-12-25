namespace RegistryWeb.Models.Api
{
    public enum MeteringDeviceAvailabilityEnum
    {
        MissingNoNeed = 1, //Отсутствует, установка не требуется
        MissingNeed, //Отсутствует, требуется установка
        Installed //Установлен
    }
}