using System;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseCommonMeeting
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "protocol_date", IsNullable = true)]
        public DateTime? ProtocolDate { get; set; } //Дата протокола общего собрания собственников помещений

        [XmlElement(ElementName = "protocol_number", IsNullable = true)]
        public string ProtocolNumber { get; set; } //Номер протокола общего собрания собственников помещений

        [XmlElement(ElementName = "protocol_file_id", IsNullable = true)]
        public int? ProtocolFileId { get; set; } //Идентификатор файла. Если идентификатор файла не указан – это означает удаление файла из анкеты.
    }
}