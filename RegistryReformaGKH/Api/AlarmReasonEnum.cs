using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum AlarmReasonEnum
    {
        [XmlEnum(Name = "1")]
        PhysicalWear = 1, //Физический износ

        [XmlEnum(Name = "2")]
        EnvironmentalInfluence, //Влияние окружающей среды

        [XmlEnum(Name = "3")]
        NaturalDisaster, //Природные катастрофы

        [XmlEnum(Name = "4")]
        TechnogenicCharacter, //Причины техногенного характера

        [XmlEnum(Name = "5")]
        Fire, //Пожар

        [XmlEnum(Name = "6")]
        Other //Иная
    }
}