using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseReport
    {
        [XmlElement(ElementName = "common", IsNullable = true)]        
        public HouseReportCommon Common { get; set; } //Отчеты по управлению. Общая информация

        [XmlElement(ElementName = "communal_service", IsNullable = true)]
        public HouseReportCommunalService CommunalService { get; set; } //Отчеты по управлению. Коммунальные услуги

        [XmlElement(ElementName = "claims_to_consumers", IsNullable = true)]
        public HouseReportClaimsToConsumers ClaimsToConsumers { get; set; } //Отчеты по управлению. Претензии по качеству работ

        [XmlElement(ElementName = "house_report_quality_of_work_claims", IsNullable = true)]
        public HouseReportQualityOfWorkClaims HouseReportQualityOfWorkClaims { get; set; } //Отчеты по управлению. Претензионно-исковая работа
    }
}