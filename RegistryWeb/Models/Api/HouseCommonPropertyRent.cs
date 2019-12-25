using System;

namespace RegistryWeb.Models.Api
{
    public class HouseCommonPropertyRent
    {
        //provider_name
        string ProviderName { get; set; } //Наименование владельца (пользователя)
        //provider_inn
        string ProviderInn { get; set; } //ИНН владельца (пользователя)
        //contract_number
        string ContractNumber { get; set; } //Номер договора
        //contract_date
        DateTime ContractDate { get; set; } //Дата договора
        //contract_start_date
        DateTime ContractStartDate { get; set; } //Дата начала действия договора
        //cost_per_month
        double CostPerMonth { get; set; } //Стоимость по договору в месяц, руб.
        //common_meeting_protocol_date
        DateTime CommonMeetingProtocolDate { get; set; } //Дата протокола общего собрания собственников помещений
        //common_meeting_protocol_number
        string CommonMeetingProtocolNumber { get; set; } //Номер протокола общего собрания собственников помещений
    }
}