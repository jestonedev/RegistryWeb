using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum СommunalServiceFillingFactEnum
    {
        [XmlEnum(Name = "1")]
        Provide = 1, //Предоставляется

        [XmlEnum(Name = "2")]
        NotProvide, //Не предоставляется

        [XmlEnum(Name = "3")]
        Terminated //Прекращено
    }
}