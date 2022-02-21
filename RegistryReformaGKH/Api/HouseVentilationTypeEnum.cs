using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseVentilationTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1, //отсутствует

        [XmlEnum(Name = "2")]
        SupplyVentilation, //Приточная вентиляция

        [XmlEnum(Name = "3")]
        ExhaustVentilation, //Вытяжная вентиляция

        [XmlEnum(Name = "4")]
        SupplyExhaustVentilation //Приточно-вытяжная вентиляция
    }
}