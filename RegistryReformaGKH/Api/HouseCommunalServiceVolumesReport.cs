using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseCommunalServiceVolumesReport
    {
        [XmlElement(ElementName = "unit_of_measurement", IsNullable = true)]
        public UnitOfMeasureEnum? UnitOfMeasurement { get; set; }

        [XmlElement(ElementName = "total_volume", IsNullable = true)]
        public double? TotalVolume { get; set; } //Общий объем потребления (нат. показ)

        [XmlElement(ElementName = "accrued_consumer", IsNullable = true)]
        public double? AccruedConsumer { get; set; } //Начислено потребителям, руб.

        [XmlElement(ElementName = "paid_by_consumers_amount", IsNullable = true)]
        public double? PaidByConsumersAmount { get; set; } //Оплачено потребителями, руб.

        [XmlElement(ElementName = "consumer_arrears", IsNullable = true)]
        public double? ConsumerArrears { get; set; } //Задолженность потребителей, руб.

        [XmlElement(ElementName = "cash_to_provider_payment", IsNullable = true)]
        public double? CashToProviderPayment { get; set; } //Начислено поставщиком (поставщиками) коммунального ресурса, руб.

        [XmlElement(ElementName = "paid_to_supplier_amount", IsNullable = true)]
        public double? PaidToSupplierAmount { get; set; } //Оплачено поставщику (поставщикам) коммунального ресурса, руб.

        [XmlElement(ElementName = "arrear_to_supplier_amount", IsNullable = true)]
        public double? ArrearToSupplierAmount { get; set; } //Задолженность перед поставщиком (поставщиками) коммунального ресурса, руб.

        [XmlElement(ElementName = "total_penalties", IsNullable = true)]
        public double? TotalPenalties { get; set; } //Суммы пени и штрафов, уплаченные поставщику (поставщикам) коммунального ресурса, руб.
    }
}