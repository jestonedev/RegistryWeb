using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseChuteTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1,//Отсутствует

        [XmlEnum(Name = "2")]
        Apartment, //Квартирные

        [XmlEnum(Name = "3")]
        Stairwell, //На лестничной клетке

        [XmlEnum(Name = "4")]
        DryCold, //Сухой (холодный)

        [XmlEnum(Name = "5")]
        Dry, //Сухой

        [XmlEnum(Name = "6")]
        Cold, //Холодный

        [XmlEnum(Name = "7")]
        FireHot, //Огневой (горячий)

        [XmlEnum(Name = "8")]
        Wet //Мокрый
    }
}