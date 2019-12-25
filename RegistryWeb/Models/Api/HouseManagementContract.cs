using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Api
{
    public class HouseManagementContract
    {
        //date_start
        DateTime DateStart { get; set; } //Дата начала обслуживания дома по договору управления
        //management_reason
        string ManagementReason { get; set; } //Основание управления
        //confirm_method_document_name
        string ConfirmMethodDocumentName { get; set; } //Наименование документа, подтверждающего выбранный способ управления
        //confirm_method_document_date
        DateTime ConfirmMethodDocumentDate { get; set; } //Дата документа, подтверждающего выбранный способ управления
        //confirm_method_document_number
        string ConfirmMethodDocumentNumber { get; set; } //Номер документа, подтверждающего выбранный способ управления
        //management_contract_date
        DateTime ManagementContractDate { get; set; } //Дата договора управления
        //management_contract_files
        List<int> ManagementContractFiles { get; set; } //Массив идентификаторов файлов договоров управления. Если идентификатор файла не указан – это означает удаление файла из анкеты.
    }
}