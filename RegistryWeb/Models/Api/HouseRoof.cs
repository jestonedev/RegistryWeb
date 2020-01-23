using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseRoof
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; } //Идентификатор крыши

        [XmlElement(ElementName = "Roof_type", IsNullable = true)]
        public HouseRoofTypeEnum? RoofType { get; set; } //Идентификатор типа крыши 

        [XmlElement(ElementName = "roofing_type", IsNullable = true)]
        public HouseRoofingTypeEnum? RoofingType { get; set; } //Идентификатор типа кровли
    }
}