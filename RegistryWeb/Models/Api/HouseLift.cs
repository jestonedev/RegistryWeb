using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseLift
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "porch_number", IsNullable = true)]
        public string PorchNumber { get; set; } //Номер подъезда

        [XmlElement(ElementName = "type", IsNullable = true)]
        public HouseLiftTypeEnum? Type { get; set; } //Идентификатор типа лифта 

        [XmlElement(ElementName = "commissioning_year", IsNullable = true)]
        public int? CommissioningYear { get; set; } //Год ввода в эксплуатацию (Формат: ‘2015’)
    }
}