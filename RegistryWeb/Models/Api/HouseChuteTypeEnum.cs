namespace RegistryWeb.Models.Api
{
    public enum HouseChuteTypeEnum
    {
        Missing = 1,//Отсутствует
        Apartment, //Квартирные
        Stairwell, //На лестничной клетке
        DryCold, //Сухой (холодный)
        Dry, //Сухой
        Cold, //Холодный
        FireHot, //Огневой (горячий)
        Wet//Мокрый
    }
}