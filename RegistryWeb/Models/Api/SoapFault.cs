using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class SoapFault
    {
        [XmlElement(ElementName = "code", IsNullable = true)]
        public int Code { get; set; } //Код результата запроса

        [XmlElement(ElementName = "name", IsNullable = true)]
        public string Name { get; set; } //Наименование 

        [XmlElement(ElementName = "description", IsNullable = true)]
        public string Description { get; set; } //Описание 
    }
}
