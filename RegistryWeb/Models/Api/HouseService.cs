using System;

namespace RegistryWeb.Models.Api
{
    internal class HouseService
    {
        //id
        int Id { get; set; }
        //name
        HouseServiceNameEnum Name { get; set; } //Идентификатор наименования работы/услуги 
        //name_other
        string NameOther { get; set; } //Наименование работы (прочая услуга). Заполняется только при HouseServiceNameEnum = 14
        //stop_reason_type
        ServiceStopReasonTypeEnum StopReasonType { get; set; } //Идентификатор типа прекращения предоставления работы (услуги) 
        //date_stop
        DateTime DateStop { get; set; }//Дата истечения срока предоставления услуги. Заполняется, если значение в поле stop_reason_type = 1 (Срок действия предоставления услуги истек).
        //stop_reason
        string StopReason { get; set; } //Основание прекращения предоставления услуг. Заполняется, если значение в поле stop_reason_type = 1 (Срок действия предоставления услуги истек).
        //report
        HouseServiceReport Report { get; set; } //Отчет по выполненным работам (услугам) 
        //costs
        HouseServicesCost Costs { get; set; } //История стоимости работы (услуги). 
    }
}