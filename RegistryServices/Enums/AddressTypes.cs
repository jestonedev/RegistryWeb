using System.Text.Json.Serialization;

namespace RegistryWeb.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AddressTypes
    {
        None = 0,        // Неопределенный адрес
        Street = 1,      // Улица
        Building = 2,    // Здание
        Premise = 3,     // Помещение внутри здания
        SubPremise = 4   // Комната внутри помещения
    }
}
