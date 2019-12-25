using System;

namespace RegistryWeb.Models.Api
{
    public class HouseAlarm
    {
        //document_date
        DateTime DocumentDate { get; set; } //Дата документа признания дома аварийным
        //document_number
        string DocumentNumber { get; set; } //Номер документа признания дома аварийным
        //reason
        AlarmReasonEnum Reason { get; set; } //Причина признания дома аварийным 
        //reason_other
        string ReasonOther { get; set; } //Причина признания дома аварийным (иная). Заполняется если reason = 6.
        //failure
        HouseAlarmFailure Failure { get; set; } //Информация при отказе от аварийности дома 
    }
}