using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseRoofTypeEnum
    {
        [XmlEnum(Name = "1")]
        Flat = 1, //Плоская

        [XmlEnum(Name = "2")]
        Pitched, //Скатная
    }
}