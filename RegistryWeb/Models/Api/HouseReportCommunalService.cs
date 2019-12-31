using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseReportCommunalService
    {
        [XmlElement(ElementName = "balance_beginning_period_consumers_overpayment")]
        double BalanceBeginningPeriodConsumersOverpayment { get; set; } //Авансовые платежи потребителей (на начало периода), руб.
        [XmlElement(ElementName = "balance_beginning_period")]
        double BalanceBeginningPeriod { get; set; } //Переходящие остатки денежных средств (на начало периода), руб.
        [XmlElement(ElementName = "balance_beginning_period_consumers_arrears")]
        double BalanceBeginningPeriodConsumersArrears { get; set; } //Задолженность потребителей (на начало периода), руб.
        [XmlElement(ElementName = "balance_ending_period_consumers_overpayment")]
        double BalanceEndingPeriodConsumersOverpayment { get; set; } //Авансовые платежи потребителей (на конец периода), руб.
        [XmlElement(ElementName = "balance_ending_period")]
        double BalanceEndingPeriod { get; set; } //Переходящие остатки денежных средств (на конец периода), руб.
        [XmlElement(ElementName = "balance_ending_period_consumers_arrears")]
        double BalanceEndingPeriodConsumersArrears { get; set; } //Задолженность потребителей (на конец периода), руб.
        [XmlElement(ElementName = "claims_received_count")]
        int ClaimsReceivedCount { get; set; } //Количество поступивших претензий по качеству предоставленных коммунальных услуг, ед.
        [XmlElement(ElementName = "claims_satisfied_count")]
        int ClaimsSatisfiedCount { get; set; } //Количество удовлетворенных претензий по качеству предоставленных коммунальных услуг, ед.
        [XmlElement(ElementName = "claims_denied_count")]
        int ClaimsDeniedCount { get; set; } //Количество претензий по качеству предоставленных коммунальных услуг, в удовлетворении которых отказано
        [XmlElement(ElementName = "produced_recalculation_amount")]
        double ProducedRecalculationAmount { get; set; } //Сумма произведенного перерасчета, руб.
    }
}