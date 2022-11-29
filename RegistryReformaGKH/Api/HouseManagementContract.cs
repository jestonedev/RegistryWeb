using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseManagementContract
    {
        [XmlElement(ElementName = "date_start")]
        public DateTime DateStart { get; set; } //Дата начала обслуживания дома по договору управления

        [XmlElement(ElementName = "management_reason", IsNullable = true)]
        public string ManagementReason { get; set; } //Основание управления

        [XmlElement(ElementName = "confirm_method_document_name", IsNullable = true)]
        public string ConfirmMethodDocumentName { get; set; } //Наименование документа, подтверждающего выбранный способ управления

        [XmlElement(ElementName = "confirm_method_document_date", IsNullable = true)]
        public DateTime? ConfirmMethodDocumentDate { get; set; } //Дата документа, подтверждающего выбранный способ управления

        [XmlElement(ElementName = "confirm_method_document_number", IsNullable = true)]
        public string ConfirmMethodDocumentNumber { get; set; } //Номер документа, подтверждающего выбранный способ управления

        [XmlElement(ElementName = "management_contract_date", IsNullable = true)]
        public DateTime? ManagementContractDate { get; set; } //Дата договора управления

        [XmlArray(ElementName = "management_contract_files", IsNullable = true)]
        public List<int> ManagementContractFiles { get; set; } //Массив идентификаторов файлов договоров управления. Если идентификатор файла не указан – это означает удаление файла из анкеты.
    }
}