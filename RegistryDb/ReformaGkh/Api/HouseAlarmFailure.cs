using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseAlarmFailure
    {
        [XmlElement(ElementName = "reason", IsNullable = true)]
        public AlarmFailureReasonEnum? Reason { get; set; } //Причина об отказе признания аварийным  

        [XmlElement(ElementName = "document_name", IsNullable = true)]
        public string DocumentName { get; set; } //Наименование документа об отказе признания дома аварийным. Поле обязательное для заполнения, если reason != 1 (если тип причины отказа не равно «Ошибочный признак состояния»)

        [XmlElement(ElementName = "document_date", IsNullable = true)]
        public DateTime? DocumentDate { get; set; } //Дата документа об отказе признания дома аварийным. Поле обязательное для заполнения, если reason != 1 (если тип причины отказа не равно «Ошибочный признак состояния»)

        [XmlElement(ElementName = "document_number", IsNullable = true)]
        public string DocumentNumber { get; set; } //Номер документа об отказе признания дома аварийным. Поле обязательное для заполнения, если reason != 1 (если тип причины отказа не равно «Ошибочный признак состояния»)

        [XmlArray(ElementName = "files", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<int> Files { get; set; } //Массив идентификаторов файлов документа об отказе от признака аварийности дома. Указывается идентификатор файла, который был загружен с помощью метода SetUploadFile. При повторном отказе от аварийности необходимо файлы заново загрузить в систему, а потом прикрепить.
    }
}