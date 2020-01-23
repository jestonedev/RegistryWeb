using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseService
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "name", IsNullable = true)]
        public HouseServiceNameEnum? Name { get; set; } //Идентификатор наименования работы/услуги 

        [XmlElement(ElementName = "name_other", IsNullable = true)]
        public string NameOther { get; set; } //Наименование работы (прочая услуга). Заполняется только при HouseServiceNameEnum = 14

        [XmlElement(ElementName = "stop_reason_type", IsNullable = true)]
        public ServiceStopReasonTypeEnum? StopReasonType { get; set; } //Идентификатор типа прекращения предоставления работы (услуги) 

        [XmlElement(ElementName = "date_stop", IsNullable = true)]
        public DateTime? DateStop { get; set; }//Дата истечения срока предоставления услуги. Заполняется, если значение в поле stop_reason_type = 1 (Срок действия предоставления услуги истек).

        [XmlElement(ElementName = "stop_reason", IsNullable = true)]
        public string StopReason { get; set; } //Основание прекращения предоставления услуг. Заполняется, если значение в поле stop_reason_type = 1 (Срок действия предоставления услуги истек).

        [XmlElement(ElementName = "report", IsNullable = true)]
        public HouseServiceReport Report { get; set; } //Отчет по выполненным работам (услугам) 

        [XmlArray(ElementName = "costs", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseServicesCost> Costs { get; set; } //История стоимости работы (услуги). 
    }
}