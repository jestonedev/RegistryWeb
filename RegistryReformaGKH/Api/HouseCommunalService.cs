using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseCommunalService
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "type", IsNullable = true)]
        public HouseCommunalServiceTypeEnum? Type { get; set; } //Вид коммунальной услуги

        [XmlElement(ElementName = "filling_fact", IsNullable = true)]
        public СommunalServiceFillingFactEnum? FillingFact { get; set; } //Факт предоставления услуги (описано в Таблица 193). Доступны все переходы кроме перехода с факта предоставления «Прекращено» на какой-либо другой факт предоставления.

        [XmlElement(ElementName = "service_method", IsNullable = true)]
        public CommunalServiceMethodEnum? ServiceMethod { get; set; } //Основание предоставления услуги

        [XmlElement(ElementName = "tariff_description", IsNullable = true)]
        public string TariffDescription { get; set; } //Описание дифференциации тарифов в случаях, предусмотренных законодательством Российской Федерации о государственном регулировании цен (тарифов)

        [XmlElement(ElementName = "tariff_description_file_id", IsNullable = true)]
        public int? TariffDescriptionFileId { get; set; } //Описание тарифа в виде файла. Идентификатор файла. Если идентификатор файла не указан – это означает удаление файла из анкеты

        [XmlElement(ElementName = "supplied_via_management_organization", IsNullable = true)]
        public bool? SuppliedViaManagementOrganization { get; set; } //Услуга предоставляется через УО. Возможные значения: true (да, услуга предоставляется через УО), false (нет, услуга не предоставляется через УО)

        [XmlElement(ElementName = "provider_inn", IsNullable = true)]
        public string ProviderInn { get; set; } //ИНН лица, осуществляющего поставку коммунального ресурса. Заполняется, если значение в поле supplied_via_management_organization  is false (услуга не предоставляется через УО)

        [XmlElement(ElementName = "provider_name", IsNullable = true)]
        public string ProviderName { get; set; } //Наименование лица, осуществляющего поставку коммунального ресурса. Заполняется, если значение в поле supplied_via_management_organization  is false (услуга не предоставляется через УО)

        [XmlElement(ElementName = "provider_additional_info", IsNullable = true)]
        public string ProviderAdditionalInfo { get; set; } //Дополнительная информация. Заполняется, если значение в поле supplied_via_management_organization is false (услуга не предоставляется через УО)

        [XmlElement(ElementName = "supply_contract_date", IsNullable = true)]
        public DateTime? SupplyContractDate { get; set; } //Дата договора на поставку коммунального ресурса. Заполняется, если значение в поле supplied_via_management_organization  is false (услуга не предоставляется через УО)

        [XmlElement(ElementName = "supply_contract_number", IsNullable = true)]
        public string SupplyContractNumber { get; set; } //Номер договора на поставку коммунального ресурса. Заполняется, если значение в поле supplied_via_management_organization  is false (услуга не предоставляется через УО)

        [XmlElement(ElementName = "costs", IsNullable = true)]
        public List<HouseCommunalServiceCost> Costs { get; set; } //История стоимости услуги. Массив данных ArrayOfHouseCommunalServiceCost возвращает тип данных  HouseCommunalServiceCost (описано в Таблица 196). История стоимости услуги обязательно для заполнения, если «Факт предоставления» = «Предоставляется» или «Прекращено». История стоимости услуги не обязательно, если «Факт предоставления» = «Предоставляется» или «Прекращено» и «Основание предоставления» = Предоставляется через прямые договоры с собственниками».

        [XmlElement(ElementName = "legal_act_of_tariff_date", IsNullable = true)]
        public DateTime? LegalActOfTariffDate { get; set; } //Дата нормативно-правового акта, устанавливающего тариф

        [XmlElement(ElementName = "legal_act_of_tariff_number", IsNullable = true)]
        public string LegalActOfTariffNumber { get; set; } //Номер нормативно-правового акта, устанавливающего тариф

        [XmlElement(ElementName = "legal_act_of_tariff_org_name", IsNullable = true)]
        public string LegalActOfTariffOrgName { get; set; } //Наименование органа, принявшего нормативно-правовой акт, устанавливающий тариф

        [XmlElement(ElementName = "consumption_norm", IsNullable = true)]
        public double? ConsumptionNorm { get; set; } //Норматив потребления коммунальной услуги в жилых помещениях

        [XmlElement(ElementName = "consumption_norm_unit_of_measurement", IsNullable = true)]
        public UnitOfMeasureEnum? ConsumptionNormUnitOfMeasurement { get; set; } //Идентификатор единицы измерения норматива потребления коммунальной услуги в жилых помещениях 

        [XmlElement(ElementName = "consumption_norm_additional_info", IsNullable = true)]
        public string ConsumptionNormAdditionalInfo { get; set; } //Дополнительная информация о нормативе потребления услуги в жилых помещениях

        [XmlElement(ElementName = "consumption_norm_on_common_needs", IsNullable = true)]
        public double? ConsumptionNormOnCommonNeeds { get; set; } //Норматив потребления коммунальной услуги на общедомовые нужды

        [XmlElement(ElementName = "consumption_norm_on_common_needs_unit_of_measurement", IsNullable = true)]
        public UnitOfMeasureEnum? ConsumptionNormOnCommonNeedsUnitOfMeasurement { get; set; } //Идентификатор единицы измерения норматива потребления коммунальной услуги на общедомовые нужды

        [XmlElement(ElementName = "consumption_norm_on_common_needs_additional_info", IsNullable = true)]
        public string ConsumptionNormOnCommonNeedsAdditionalInfo { get; set; } //Дополнительная информация о нормативе потребления услуги на общедомовые нужды

        [XmlElement(ElementName = "normative_acts", IsNullable = true)]
        public List<HouseCommunalServiceNormativeAct> NormativeActs { get; set; } //Нормативно-правовой акт, устанавливающий норматив потребления коммунальной услуги (заполняется по каждому нормативному правовому акту)

        [XmlElement(ElementName = "stop_reason_type", IsNullable = true)]
        public ServiceStopReasonTypeEnum? StopReasonType { get; set; } //Идентификатор типа прекращения предоставления услуги. Для основной коммунальной услуги (is_default is true)  можно указать только значение ServiceStopReasonTypeEnum = 1

        [XmlElement(ElementName = "date_stop", IsNullable = true)]
        public DateTime? DateStop { get; set; } //Дата истечения срока предоставления услуги. Заполняется, если значение в поле stop_reason_type = 1 (Срок действия предоставления услуги истек)

        [XmlElement(ElementName = "stop_reason", IsNullable = true)]
        public string StopReason { get; set; } //Основание прекращения предоставления услуг. Заполняется, если значение в поле stop_reason_type = 1 (Срок действия предоставления услуги истек)

        [XmlElement(ElementName = "volumes_report", IsNullable = true)]
        public HouseCommunalServiceVolumesReport VolumesReport { get; set; } //Отчет по управлению. Объемы по коммунальным услугам (описано в Таблица 210). Объемы по коммунальной услуге не заполняются, если  «Описание предоставления услуги» = «Предоставляется через прямые договоры с собственниками» или «Факт предоставления услуги» = «Не предоставляется» объемы по коммунальной услуге не заполняются

        [XmlElement(ElementName = "is_default", IsNullable = true)]
        public bool? IsDefault { get; set; } //Признак определения основная (is_default is true) коммунальная услуга или пользовательская (is_default is false). Нельзя удалить основные услуги – их 6 штук. Нельзя менять значение в поле «type(Вид коммунальной услуги)» для основных КУ, такая возможность есть только для пользовательских КУ.Пока не заполнено поле «filling_fact(Факт предоставления услуги)» для всех 6ти КУ, нельзя добавить пользовательскую КУ.
    }
}