using System;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseOverhaul
    {
        [XmlElement(ElementName = "provider_inn", IsNullable = true)]
        public string ProviderInn { get; set; } //ИНН владельца специального счета

        [XmlElement(ElementName = "provider_name", IsNullable = true)]
        public string ProviderName { get; set; } //Наименование владельца специального счета

        [XmlElement(ElementName = "common_meeting_protocol_date", IsNullable = true)]
        public DateTime? CommonMeetingProtocolDate { get; set; } //Дата протокола общего собрания собственников помещений, на котором принято решение о способе формирования фонда капитального ремонта

        [XmlElement(ElementName = "common_meeting_protocol_number", IsNullable = true)]
        public string CommonMeetingProtocolNumber { get; set; } //Номер протокола общего собрания собственников помещений, на котором принято решение о способе формирования фонда капитального ремонта

        [XmlElement(ElementName = "payment_amount_for_1sm", IsNullable = true)]
        public double? PaymentAmountFor1sm { get; set; } //Размер взноса на капитальный ремонт на 1 кв. м в соответствии с решением общего собрания собственников помещений в многоквартирном доме, руб.

        [XmlElement(ElementName = "additional_info", IsNullable = true)]
        public string AdditionalInfo { get; set; } //Дополнительная информация
    }
}