using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseCommonProperty
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "name", IsNullable = true)]
        public string Name { get; set; } //Наименование объекта общего имущества

        [XmlElement(ElementName = "function", IsNullable = true)]
        public string Function { get; set; } //Назначение объекта общего имущества

        [XmlElement(ElementName = "area", IsNullable = true)]
        public double? Area { get; set; } //Площадь объекта общего имущества (заполняется в отношении помещений и земельных участков), кв.м

        [XmlElement(ElementName = "rent", IsNullable = true)]
        public HouseCommonPropertyRent Rent { get; set; } //Общее имущество сдается в аренду (в пользование)
    }
}