using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum AlarmFailureReasonEnum
    {
        [XmlEnum(Name = "1")]
        ErrorCondition = 1, //Ошибочный признак состояния

        [XmlEnum(Name = "2")]
        Reconstruction, //Реконструкция

        [XmlEnum(Name = "3")]
        Habitable//Признание пригодным для проживания
    }
}