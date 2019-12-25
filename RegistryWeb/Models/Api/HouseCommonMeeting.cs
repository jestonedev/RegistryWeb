using System;

namespace RegistryWeb.Models.Api
{
    public class HouseCommonMeeting
    {
        //id
        int Id { get; set; }
        //protocol_date
        DateTime ProtocolDate { get; set; } //Дата протокола общего собрания собственников помещений
        //protocol_number
        string ProtocolNumber { get; set; } //Номер протокола общего собрания собственников помещений
        //protocol_file_id
        int ProtocolFileId { get; set; } //Идентификатор файла. Если идентификатор файла не указан – это означает удаление файла из анкеты.
    }
}