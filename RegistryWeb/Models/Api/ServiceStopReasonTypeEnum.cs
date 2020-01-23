using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum ServiceStopReasonTypeEnum
    {
        [XmlEnum(Name = "1")]
        TimeExpired = 1, //Срок действия предоставления услуги истек

        [XmlEnum(Name = "2")]
        EnabledByMistake //Услуга была включена в список предоставляемых услуг по ошибке
    }
}