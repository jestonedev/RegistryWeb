using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseAdditionalEquipment
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "type", IsNullable = true)]
        public string Type { get; set; }

        [XmlElement(ElementName = "description", IsNullable = true)]
        public string Description { get; set; }
    }
}