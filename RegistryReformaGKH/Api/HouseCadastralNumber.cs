using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseCadastralNumber
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; } //Идентификатор кадастрового номера

        [XmlElement(ElementName = "cadastral_number", IsNullable = true)]
        public string CadastralNumber { get; set; } //Кадастровый номер
    }
}