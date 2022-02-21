using System;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseCommunalServiceNormativeAct
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "document_date", IsNullable = true)]
        public DateTime? DocumentDate { get; set; } //Дата нормативного правового акта, устанавливающего норматив потребления коммунальной услуги

        [XmlElement(ElementName = "document_number", IsNullable = true)]
        public string DocumentNumber { get; set; } //Номер нормативного правового акта, устанавливающего норматив потребления коммунальной услуги

        [XmlElement(ElementName = "document_organization_name", IsNullable = true)]
        public string DocumentOrganizationName { get; set; } //Наименование принявшего акт органа
    }
}