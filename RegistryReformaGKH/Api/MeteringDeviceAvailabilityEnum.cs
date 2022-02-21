using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum MeteringDeviceAvailabilityEnum
    {
        [XmlEnum(Name = "1")]
        MissingNoNeed = 1, //Отсутствует, установка не требуется

        [XmlEnum(Name = "2")]
        MissingNeed, //Отсутствует, требуется установка

        [XmlEnum(Name = "3")]
        Installed //Установлен
    }
}