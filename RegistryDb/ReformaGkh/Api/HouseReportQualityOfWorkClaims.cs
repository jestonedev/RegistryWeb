using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseReportQualityOfWorkClaims
    {
        [XmlElement(ElementName = "claims_received_count")]
        public int ClaimsReceivedCount { get; set; } //Количество поступивших претензий по качеству работ, ед.

        [XmlElement(ElementName = "claims_satisfied_count")]
        public int ClaimsSatisfiedCount { get; set; } //Количество удовлетворенных претензий по качеству работ, ед.

        [XmlElement(ElementName = "claims_denied_count")]
        public int ClaimsDeniedCount { get; set; } //Количество претензий по качеству работ, в удовлетворении которых отказано, ед.

        [XmlElement(ElementName = "produced_recalculation_amount")]
        public double ProducedRecalculationAmount { get; set; } //Сумма произведенного перерасчета, руб.
    }
}