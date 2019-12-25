using System;

namespace RegistryWeb.Models.Api
{
    public class HouseOverhaul
    {
        //provider_inn
        string ProviderInn { get; set; } //ИНН владельца специального счета
        //provider_name
        string ProviderName { get; set; } //Наименование владельца специального счета
        //common_meeting_protocol_date
        DateTime CommonMeetingProtocolDate { get; set; } //Дата протокола общего собрания собственников помещений, на котором принято решение о способе формирования фонда капитального ремонта
        //common_meeting_protocol_number
        string CommonMeetingProtocolNumber { get; set; } //Номер протокола общего собрания собственников помещений, на котором принято решение о способе формирования фонда капитального ремонта
        //payment_amount_for_1sm
        double PaymentAmountFor1sm { get; set; } //Размер взноса на капитальный ремонт на 1 кв. м в соответствии с решением общего собрания собственников помещений в многоквартирном доме, руб.
        //additional_info
        string AdditionalInfo { get; set; } //Дополнительная информация
    }
}