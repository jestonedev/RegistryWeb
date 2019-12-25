using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Api
{
    public class HouseCommunalService
    {
        //id
        int Id { get; set; }
        //type
        HouseCommunalServiceTypeEnum Type { get; set; } //Вид коммунальной услуги
        //filling_fact
        СommunalServiceFillingFactEnum FillingFact { get; set; } //Факт предоставления услуги (описано в Таблица 193). Доступны все переходы кроме перехода с факта предоставления «Прекращено» на какой-либо другой факт предоставления.
        //service_method
        CommunalServiceMethodEnum ServiceMethod { get; set; } //Основание предоставления услуги
        //tariff_description
        string TariffDescription { get; set; } //Описание дифференциации тарифов в случаях, предусмотренных законодательством Российской Федерации о государственном регулировании цен (тарифов)
        //tariff_description_file_id
        int TariffDescriptionFileId { get; set; } //Описание тарифа в виде файла. Идентификатор файла. Если идентификатор файла не указан – это означает удаление файла из анкеты
        //supplied_via_management_organization
        bool SuppliedViaManagementOrganization { get; set; } //Услуга предоставляется через УО. Возможные значения: true (да, услуга предоставляется через УО), false (нет, услуга не предоставляется через УО)
        //provider_inn
        string ProviderInn { get; set; } //ИНН лица, осуществляющего поставку коммунального ресурса. Заполняется, если значение в поле supplied_via_management_organization  is false (услуга не предоставляется через УО)
        //provider_name
        string ProviderName { get; set; } //Наименование лица, осуществляющего поставку коммунального ресурса. Заполняется, если значение в поле supplied_via_management_organization  is false (услуга не предоставляется через УО)
        //provider_additional_info
        string ProviderAdditionalInfo { get; set; } //Дополнительная информация. Заполняется, если значение в поле supplied_via_management_organization is false (услуга не предоставляется через УО)
        //supply_contract_date
        DateTime SupplyContractDate { get; set; } //Дата договора на поставку коммунального ресурса. Заполняется, если значение в поле supplied_via_management_organization  is false (услуга не предоставляется через УО)
        //supply_contract_number
        string SupplyContractNumber { get; set; } //Номер договора на поставку коммунального ресурса. Заполняется, если значение в поле supplied_via_management_organization  is false (услуга не предоставляется через УО)
        //costs
        List<HouseCommunalServiceCost> Costs { get; set; } //История стоимости услуги. Массив данных ArrayOfHouseCommunalServiceCost возвращает тип данных  HouseCommunalServiceCost (описано в Таблица 196). История стоимости услуги обязательно для заполнения, если «Факт предоставления» = «Предоставляется» или «Прекращено». История стоимости услуги не обязательно, если «Факт предоставления» = «Предоставляется» или «Прекращено» и «Основание предоставления» = Предоставляется через прямые договоры с собственниками».
        //legal_act_of_tariff_date
        DateTime LegalActOfTariffDate { get; set; } //Дата нормативно-правового акта, устанавливающего тариф
        //legal_act_of_tariff_number
        string LegalActOfTariffNumber { get; set; } //Номер нормативно-правового акта, устанавливающего тариф
        //legal_act_of_tariff_org_name
        string LegalActOfTariffOrgName { get; set; } //Наименование органа, принявшего нормативно-правовой акт, устанавливающий тариф
        //consumption_norm
        double ConsumptionNorm { get; set; } //Норматив потребления коммунальной услуги в жилых помещениях
        //consumption_norm_unit_of_measurement
        UnitOfMeasureEnum ConsumptionNormUnitOfMeasurement { get; set; } //Идентификатор единицы измерения норматива потребления коммунальной услуги в жилых помещениях 
        //consumption_norm_additional_info
        string ConsumptionNormAdditionalInfo { get; set; } //Дополнительная информация о нормативе потребления услуги в жилых помещениях
        //consumption_norm_on_common_needs
        double ConsumptionNormOnCommonNeeds { get; set; } //Норматив потребления коммунальной услуги на общедомовые нужды
        //consumption_norm_on_common_needs_unit_of_measurement
        UnitOfMeasureEnum ConsumptionNormOnCommonNeedsUnitOfMeasurement { get; set; } //Идентификатор единицы измерения норматива потребления коммунальной услуги на общедомовые нужды
        //consumption_norm_on_common_needs_additional_info
        string ConsumptionNormOnCommonNeedsAdditionalInfo { get; set; } //Дополнительная информация о нормативе потребления услуги на общедомовые нужды
        //normative_acts
        List<HouseCommunalServiceNormativeAct> NormativeActs { get; set; } //Нормативно-правовой акт, устанавливающий норматив потребления коммунальной услуги (заполняется по каждому нормативному правовому акту)
        //stop_reason_type
        ServiceStopReasonTypeEnum StopReasonType { get; set; } //Идентификатор типа прекращения предоставления услуги. Для основной коммунальной услуги (is_default is true)  можно указать только значение ServiceStopReasonTypeEnum = 1
        //date_stop
        DateTime DateStop { get; set; } //Дата истечения срока предоставления услуги. Заполняется, если значение в поле stop_reason_type = 1 (Срок действия предоставления услуги истек)
        //stop_reason
        string StopReason { get; set; } //Основание прекращения предоставления услуг. Заполняется, если значение в поле stop_reason_type = 1 (Срок действия предоставления услуги истек)
        //volumes_report
        HouseCommunalServiceVolumesReport VolumesReport { get; set; } //Отчет по управлению. Объемы по коммунальным услугам (описано в Таблица 210). Объемы по коммунальной услуге не заполняются, если  «Описание предоставления услуги» = «Предоставляется через прямые договоры с собственниками» или «Факт предоставления услуги» = «Не предоставляется» объемы по коммунальной услуге не заполняются
        //is_default
        bool IsDefault { get; set; } //Признак определения основная (is_default is true) коммунальная услуга или пользовательская (is_default is false). Нельзя удалить основные услуги – их 6 штук. Нельзя менять значение в поле «type(Вид коммунальной услуги)» для основных КУ, такая возможность есть только для пользовательских КУ.Пока не заполнено поле «filling_fact(Факт предоставления услуги)» для всех 6ти КУ, нельзя добавить пользовательскую КУ.

    }
}