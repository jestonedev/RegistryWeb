using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class FullAddress
    {
        [XmlElement(ElementName = "region_guid", IsNullable = true)]
        public string RegionGuid { get; set; } //Идентификатор региона (GUID по ФИАС, aolevel = 1)
        [XmlElement(ElementName = "region_formal_name", IsNullable = true)]
        public string RegionFormalName { get; set; } //Формализованное наименование региона (ФИАС
        [XmlElement(ElementName = "region_short_name", IsNullable = true)]
        public string RegionShortName { get; set; } //Краткое наименование типа региона (ФИАС)
        [XmlElement(ElementName = "region_code", IsNullable = true)]
        public string RegionCode { get; set; } //Код региона из КЛАДР 4.0.
        [XmlElement(ElementName = "area_guid", IsNullable = true)]
        public string AreaGuid { get; set; } //Идентификатор района (GUID по ФИАС, aolevel = 3)
        [XmlElement(ElementName = "area_formal_name", IsNullable = true)]
        public string AreaFormalName { get; set; } //Формализованное наименование района (ФИАС)
        [XmlElement(ElementName = "area_short_name", IsNullable = true)]
        public string AreaShortName { get; set; } //Краткое наименование типа района (ФИАС)
        [XmlElement(ElementName = "area_code", IsNullable = true)]
        public string AreaCode { get; set; } //Код района из КЛАДР 4.0.
        [XmlElement(ElementName = "city1_guid", IsNullable = true)]
        public string City1Guid { get; set; } //Идентификатор города (GUID по ФИАС, aolevel = 4). 
        [XmlElement(ElementName = "city1_formal_name", IsNullable = true)]
        public string City1FormalName { get; set; } //Формализованное наименование города (ФИАС)
        [XmlElement(ElementName = "city1_short_name", IsNullable = true)]
        public string City1ShortName { get; set; } //Краткое наименование типа города (ФИАС)
        [XmlElement(ElementName = "city1_code", IsNullable = true)]
        public string City1Code { get; set; } //Код города из КЛАДР 4.0.
        [XmlElement(ElementName = "city2_guid", IsNullable = true)]
        public string City2Guid { get; set; } //Идентификатор внутригородской территории (GUID по ФИАС, aolevel = 5). 
        [XmlElement(ElementName = "city2_formal_name", IsNullable = true)]
        public string City2FormalName { get; set; } //Формализованное наименование внутригородской территории (ФИАС)
        [XmlElement(ElementName = "city2_short_name", IsNullable = true)]
        public string City2ShortName { get; set; } //Краткое наименование внутригородской территории (ФИАС)
        [XmlElement(ElementName = "city2_code", IsNullable = true)]
        public string City2Code { get; set; } //Код внутригородской территории из КЛАДР 4.0.
        [XmlElement(ElementName = "city3_guid", IsNullable = true)]
        public string City3Guid { get; set; } //Идентификатор населенного пункта (GUID по ФИАС, aolevel = 6).
        [XmlElement(ElementName = "City3_formal_name", IsNullable = true)]
        public string City3FormalName { get; set; } //Формализованное наименование населенного пункта (ФИАС)
        [XmlElement(ElementName = "city3_short_name", IsNullable = true)]
        public string City3ShortName { get; set; } //Краткое наименование населенного пункта (ФИАС)
        [XmlElement(ElementName = "city3_code", IsNullable = true)]
        public string City3Code { get; set; } //Код населенного пункта из КЛАДР 4.0.
        [XmlElement(ElementName = "street_guid", IsNullable = true)]
        public string StreetGuid { get; set; } //Идентификатор улицы (GUID по ФИАС, aolevel = 7)
        [XmlElement(ElementName = "street_formal_name", IsNullable = true)]
        public string StreetFormalName { get; set; } //Формализованное наименование улицы (ФИАС)
        [XmlElement(ElementName = "street_short_name", IsNullable = true)]
        public string StreetShortName { get; set; } //Краткое наименование типа улицы (ФИАС)
        [XmlElement(ElementName = "street_code", IsNullable = true)]
        public string StreetCode { get; set; } //Код улицы из КЛАДР 4.0.
        [XmlElement(ElementName = "additional_territory", IsNullable = true)]
        public string AdditionalTerritory { get; set; } //Идентификатор дополнительной территории (GUID по ФИАС, aolevel = 90)
        [XmlElement(ElementName = "additional_territory_formal_name", IsNullable = true)]
        public string AdditionalTerritoryFormalName { get; set; } //Формализованное наименование дополнительной территории (ФИАС)
        [XmlElement(ElementName = "additional_territory_short_name", IsNullable = true)]
        public string AdditionalTerritoryShortName { get; set; } //Краткое наименование типа дополнительной территории (ФИАС)
        [XmlElement(ElementName = "additional_territory_code", IsNullable = true)]
        public string AdditionalTerritoryCode { get; set; } //Код дополнительной территории из КЛАДР 4.0.
        [XmlElement(ElementName = "houseguid", IsNullable = true)]
        public string Houseguid { get; set; } //Идентификатор дома GUID из ФИАС, либо сгенерированный внутренний GUID
        [XmlElement(ElementName = "house_number", IsNullable = true)]
        public string HouseNumber { get; set; } //Номер дома
        [XmlElement(ElementName = "building", IsNullable = true)]
        public string Building { get; set; } //Строение
        [XmlElement(ElementName = "block", IsNullable = true)]
        public string Block { get; set; } //Корпус. Доступно для заполнения, если не заполнены «Литера» и «Сооружение»
        [XmlElement(ElementName = "letter", IsNullable = true)]
        public string Letter { get; set; } //Литера. Доступно для заполнения, если не заполнены «Строение» и «Сооружение»
        [XmlElement(ElementName = "structure", IsNullable = true)]
        public string Structure { get; set; } //Сооружение. Доступно для заполнения, если не заполнены «Строение» и «Литера»
    }
}