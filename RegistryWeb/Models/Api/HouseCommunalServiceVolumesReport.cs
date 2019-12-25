namespace RegistryWeb.Models.Api
{
    public class HouseCommunalServiceVolumesReport
    {
        //unit_of_measurement
        UnitOfMeasureEnum UnitOfMeasurement { get; set; }
        //total_volume
        double TotalVolume { get; set; } //Общий объем потребления (нат. показ)
        //accrued_consumer
        double AccruedConsumer { get; set; } //Начислено потребителям, руб.
        //paid_by_consumers_amount
        double PaidByConsumersAmount { get; set; } //Оплачено потребителями, руб.
        //consumer_arrears
        double ConsumerArrears { get; set; } //Задолженность потребителей, руб.
        //cash_to_provider_payment
        double CashToProviderPayment { get; set; } //Начислено поставщиком (поставщиками) коммунального ресурса, руб.
        //paid_to_supplier_amount
        double PaidToSupplierAmount { get; set; } //Оплачено поставщику (поставщикам) коммунального ресурса, руб.
        //arrear_to_supplier_amount
        double ArrearToSupplierAmount { get; set; } //Задолженность перед поставщиком (поставщиками) коммунального ресурса, руб.
        //total_penalties
        double TotalPenalties { get; set; } //Суммы пени и штрафов, уплаченные поставщику (поставщикам) коммунального ресурса, руб.
    }
}