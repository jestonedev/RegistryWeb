using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseMeterTypeEnum
    {
        [XmlEnum(Name = "1")]
        WithoutInterface = 1, //Без интерфейса передачи данных

        [XmlEnum(Name = "2")]
        WithInterface //С интерфейсом передачи данных
    }
}