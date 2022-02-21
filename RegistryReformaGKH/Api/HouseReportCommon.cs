using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseReportCommon
    {
        [XmlElement(ElementName = "cash_balance_beginning_period_consumers_overpayment", IsNullable = true)]
        public double? CashBalanceBeginningPeriodConsumersOverpayment { get; set; } //Авансовые платежи потребителей (на начало периода), руб.

        [XmlElement(ElementName = "cash_balance_beginning_period", IsNullable = true)]
        public double? CashBalanceBeginningPeriod { get; set; } //Переходящие остатки денежных средств (на начало периода), руб.

        [XmlElement(ElementName = "cash_balance_beginning_period_consumers_arrears", IsNullable = true)]
        public double? CashBalanceBeginningPeriodConsumersArrears { get; set; } //Задолженность потребителей (на начало периода), руб.

        [XmlElement(ElementName = "charged_for_services", IsNullable = true)]
        public double? ChargedForServices { get; set; } //Начислено за услуги (работы) по содержанию и текущему ремонту, руб. (всего)

        [XmlElement(ElementName = "charged_for_maintenance_of_house", IsNullable = true)]
        public double? ChargedFforMaintenanceOfHouse { get; set; } //Начислено за услуги (работы) по содержанию и текущему ремонту, в том числе, за содержание дома, руб.

        [XmlElement(ElementName = "charged_for_maintenance_work", IsNullable = true)]
        public double? ChargedForMaintenanceWork { get; set; } //Начислено за услуги (работы) по содержанию и текущему ремонту, в том числе, за текущий ремонт, руб.

        [XmlElement(ElementName = "charged_for_management_service", IsNullable = true)]
        public double? ChargedForManagementService { get; set; } //Начислено за услуги (работы) по содержанию и текущему ремонту, в том числе, за услуги управления, руб.

        [XmlElement(ElementName = "received_cash", IsNullable = true)]
        public double? ReceivedCash { get; set; } //Получено денежных средств, руб. (всего)

        [XmlElement(ElementName = "received_cash_from_owners", IsNullable = true)]
        public double? ReceivedCashFromOwners { get; set; } //Получено денежных средств, в т. Ч, денежных средств от собственников/нанимателей помещений, руб.

        [XmlElement(ElementName = "received_target_payment_from_owners", IsNullable = true)]
        public double? ReceivedTargetPaymentFromOwners { get; set; } //Получено денежных средств, в т. Ч, целевых взносов от собственников/нанимателей помещений, руб.

        [XmlElement(ElementName = "received_subsidies", IsNullable = true)]
        public double? ReceivedSubsidies { get; set; } //Получено денежных средств, в т. Ч, субсидий, руб.

        [XmlElement(ElementName = "received_from_use_of_common_property", IsNullable = true)]
        public double? ReceivedFromUseOfCommonProperty { get; set; } //Получено денежных средств, в т. Ч, денежных средств от использования общего имущества, руб.

        [XmlElement(ElementName = "received_from_other", IsNullable = true)]
        public double? ReceivedFromOther { get; set; } //Получено денежных средств, в т. Ч, прочие поступления, руб.

        [XmlElement(ElementName = "cash_total", IsNullable = true)]
        public double? CashTotal { get; set; } //Всего денежных средств с учетом остатков, руб.

        [XmlElement(ElementName = "cash_balance_ending_period_consumers_overpayment", IsNullable = true)]
        public double? CashBalanceEndingPeriodConsumersOverpayment { get; set; } //Авансовые платежи потребителей на конец периода, руб.

        [XmlElement(ElementName = "cash_balance_ending_period", IsNullable = true)]
        public double? CashBalanceEndingPeriod { get; set; } //Переходящие остатки денежных средств на конец периода, руб.

        [XmlElement(ElementName = "cash_balance_ending_period_consumers_arrears", IsNullable = true)]
        public double? CashBalanceEndingPeriodConsumersArrears { get; set; } //Задолженность потребителей на конец периода, руб.
    }
}