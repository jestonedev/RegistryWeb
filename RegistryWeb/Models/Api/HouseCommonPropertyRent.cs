using System;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseCommonPropertyRent
    {
        [XmlElement(ElementName = "provider_name", IsNullable = true)]
        public string ProviderName { get; set; } //Наименование владельца (пользователя)

        [XmlElement(ElementName = "provider_inn", IsNullable = true)]
        public string ProviderInn { get; set; } //ИНН владельца (пользователя)

        [XmlElement(ElementName = "contract_number", IsNullable = true)]
        public string ContractNumber { get; set; } //Номер договора

        [XmlElement(ElementName = "contract_date", IsNullable = true)]
        public DateTime? ContractDate { get; set; } //Дата договора

        [XmlElement(ElementName = "contract_start_date", IsNullable = true)]
        public DateTime? ContractStartDate { get; set; } //Дата начала действия договора

        [XmlElement(ElementName = "cost_per_month", IsNullable = true)]
        public double? CostPerMonth { get; set; } //Стоимость по договору в месяц, руб.

        [XmlElement(ElementName = "common_meeting_protocol_date", IsNullable = true)]
        public DateTime? CommonMeetingProtocolDate { get; set; } //Дата протокола общего собрания собственников помещений

        [XmlElement(ElementName = "common_meeting_protocol_number", IsNullable = true)]
        public string CommonMeetingProtocolNumber { get; set; } //Номер протокола общего собрания собственников помещений
    }
}