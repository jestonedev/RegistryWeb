using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseFacade
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "type", IsNullable = true)]
        public HouseFacadeTypeEnum? Type { get; set; }
    }
}