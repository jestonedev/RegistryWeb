using System;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseAlarm
    {
        [XmlElement(ElementName = "document_date", IsNullable = true)]
        public DateTime? DocumentDate { get; set; } //Дата документа признания дома аварийным

        [XmlElement(ElementName = "document_number", IsNullable = true)]
        public string DocumentNumber { get; set; } //Номер документа признания дома аварийным

        [XmlElement(ElementName = "reason", IsNullable = true)]
        public AlarmReasonEnum? Reason { get; set; } //Причина признания дома аварийным 

        [XmlElement(ElementName = "reason_other", IsNullable = true)]
        public string ReasonOther { get; set; } //Причина признания дома аварийным (иная). Заполняется если reason = 6.

        [XmlElement(ElementName = "failure", IsNullable = true)]
        public HouseAlarmFailure Failure { get; set; } //Информация при отказе от аварийности дома 
    }
}