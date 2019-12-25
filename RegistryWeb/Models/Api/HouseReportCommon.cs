namespace RegistryWeb.Models.Api
{
    public class HouseReportCommon
    {
        //cash_balance_beginning_period_consumers_overpayment
        double CashBalanceBeginningPeriodConsumersOverpayment { get; set; } //Авансовые платежи потребителей (на начало периода), руб.
        //cash_balance_beginning_period
        double CashBalanceBeginningPeriod { get; set; } //Переходящие остатки денежных средств (на начало периода), руб.
        //cash_balance_beginning_period_consumers_arrears
        double CashBalanceBeginningPeriodConsumersArrears { get; set; } //Задолженность потребителей (на начало периода), руб.
        //charged_for_services
        double ChargedForServices { get; set; } //Начислено за услуги (работы) по содержанию и текущему ремонту, руб. (всего)
        //charged_for_maintenance_of_house
        double ChargedFforMaintenanceOfHouse { get; set; } //Начислено за услуги (работы) по содержанию и текущему ремонту, в том числе, за содержание дома, руб.
        //charged_for_maintenance_work
        double ChargedForMaintenanceWork { get; set; } //Начислено за услуги (работы) по содержанию и текущему ремонту, в том числе, за текущий ремонт, руб.
        //charged_for_management_service
        double ChargedForManagementService { get; set; } //Начислено за услуги (работы) по содержанию и текущему ремонту, в том числе, за услуги управления, руб.
        //received_cash
        double ReceivedCash { get; set; } //Получено денежных средств, руб. (всего)
        //received_cash_from_owners
        double ReceivedCashFromOwners { get; set; } //Получено денежных средств, в т. Ч, денежных средств от собственников/нанимателей помещений, руб.
        //received_target_payment_from_owners
        double ReceivedTargetPaymentFromOwners { get; set; } //Получено денежных средств, в т. Ч, целевых взносов от собственников/нанимателей помещений, руб.
        //received_subsidies
        double ReceivedSubsidies { get; set; } //Получено денежных средств, в т. Ч, субсидий, руб.
        //received_from_use_of_common_property
        double ReceivedFromUseOfCommonProperty { get; set; } //Получено денежных средств, в т. Ч, денежных средств от использования общего имущества, руб.
        //received_from_other
        double ReceivedFromOther { get; set; } //Получено денежных средств, в т. Ч, прочие поступления, руб.
        //cash_total
        double CashTotal { get; set; } //Всего денежных средств с учетом остатков, руб.
        //cash_balance_ending_period_consumers_overpayment
        double CashBalanceEndingPeriodConsumersOverpayment { get; set; } //Авансовые платежи потребителей на конец периода, руб.
        //cash_balance_ending_period
        double CashBalanceEndingPeriod { get; set; } //Переходящие остатки денежных средств на конец периода, руб.
        //cash_balance_ending_period_consumers_arrears
        double CashBalanceEndingPeriodConsumersArrears { get; set; } //Задолженность потребителей на конец периода, руб.


    }
}