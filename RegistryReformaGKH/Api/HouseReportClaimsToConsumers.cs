using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseReportClaimsToConsumers
    {
        [XmlElement(ElementName = "sent_claims_count")]
        public int SentClaimsCount { get; set; } //Направлено претензий потребителям- должникам, ед.

        [XmlElement(ElementName = "filed_actions_count")]
        public int FiledActionsCount { get; set; } //Направлено исковых заявлений, ед.

        [XmlElement(ElementName = "received_cash_amount")]
        public double ReceivedCashAmount { get; set; } //Получено денежных средств по результатам претензионно-исковой работы, ед.
    }
}