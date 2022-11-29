using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseDrainageTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1, //Отсутствует

        [XmlEnum(Name = "2")]
        ExternalDrains, // Наружные водостоки

        [XmlEnum(Name = "3")]
        InternalDrains, // Внутренние водостоки

        [XmlEnum(Name = "4")]
        Mixed // Смешанные
    }
}