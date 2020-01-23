using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseProfileData988
    {
        [XmlElement(ElementName = "area_total", IsNullable = true)]
        public double? AreaTotal { get; set; }  //Общая площадь дома, кв.м

        [XmlElement(ElementName = "area_residental", IsNullable = true)]
        public double? AreaResidental { get; set; } //Общая площадь дома, в т.ч. жилых помещений, кв.м

        [XmlElement(ElementName = "area_non_residental", IsNullable = true)]
        public double? AreaNonResidental { get; set; } //Общая площадь дома, в т.ч. нежилых помещений, кв.м

        [XmlElement(ElementName = "area_common_property", IsNullable = true)]
        public double? AreaCommonProperty { get; set; } //Общая площадь дома, в т.ч. помещений, входящих в состав общего имущества, кв.м.

        [XmlElement(ElementName = "is_alarm", IsNullable = true)]
        public bool? IsAlarm { get; set; } //Признак аварийности. Возможные значения: true (да, дом аварийный), false (нет, дом не аварийный). Значение по умолчанию: «Нет». Информация по аварийности или при отказе от аварийности заполняется в теге «alarm_info».

        [XmlElement(ElementName = "method_of_forming_overhaul_fund", IsNullable = true)]
        public FormingOverhaulFundEnum? MethodOfFormingOverhaulFund { get; set; } //Способ формирования фонда капитального ремонта 

        [XmlElement(ElementName = "exploitation_start_year", IsNullable = true)]
        public int? ExploitationStartYear { get; set; } //Год ввода в эксплуатацию (Формат: ‘2015’)

        [XmlElement(ElementName = "project_type", IsNullable = true)]
        public string ProjectType { get; set; } //Серия, тип постройки здания

        [XmlElement(ElementName = "built_year", IsNullable = true)]
        public int? BuiltYear { get; set; } //Год постройки. (Формат: ‘2015’)

        [XmlElement(ElementName = "house_type", IsNullable = true)]
        public HouseType988Enum? HouseType { get; set; } //Тип дома (описано в Таблица 157)

        [XmlElement(ElementName = "floor_count_max", IsNullable = true)]
        public int? FloorCountMax { get; set; } //Количество этажей, наибольшее, ед.

        [XmlElement(ElementName = "floor_count_min", IsNullable = true)]
        public int? FloorCountMin { get; set; } //Количество этажей, наименьшее, ед.

        [XmlElement(ElementName = "entrance_count", IsNullable = true)]
        public int? EntranceCount { get; set; } //Количество подъездов

        [XmlElement(ElementName = "elevators_count", IsNullable = true)]
        public int? ElevatorsCount { get; set; } //Количество лифтов

        [XmlElement(ElementName = "flats_count", IsNullable = true)]
        public int? FlatsCount { get; set; } //Количество квартир

        [XmlElement(ElementName = "living_quarters_count", IsNullable = true)]
        public int? LivingQuartersCount { get; set; } //Количество помещений, в том числе, жилых, ед.

        [XmlElement(ElementName = "not_living_quarters_count", IsNullable = true)]
        public int? NotLivingQuartersCount { get; set; } //Количество помещений, в том числе, нежилых, ед.

        [XmlElement(ElementName = "area_land", IsNullable = true)]
        public double? AreaLand { get; set; } //Площадь земельного участка, входящего в состав общего имущества в многоквартирном доме, кв.м

        [XmlElement(ElementName = "parking_square", IsNullable = true)]
        public double? ParkingSquare { get; set; } //Площадь парковки в границах земельного участка, кв.м

        [XmlElement(ElementName = "energy_efficiency", IsNullable = true)]
        public HouseEnergyEfficiencyClass988Enum? EnergyEfficiency { get; set; } //Класс энергетической эффективности

        [XmlElement(ElementName = "additional_info", IsNullable = true)]
        public string AdditionalInfo { get; set; } //Дополнительная информация

        [XmlElement(ElementName = "has_playground", IsNullable = true)]
        public bool? HasPlayground { get; set; } //Элементы благоустройства  (детская площадка).  Возможные значения true (да, детская площадка имеется), false (нет, не имеется).

        [XmlElement(ElementName = "has_sportsground", IsNullable = true)]
        public bool? HasSportsground { get; set; } //Элементы благоустройства (спортивная площадка). Возможные значения true (да, спортивная площадка имеется) , false (нет, не имеется).

        [XmlElement(ElementName = "other_beautification", IsNullable = true)]
        public string OtherBeautification { get; set; } //Элементы благоустройства  (другое)

        [XmlElement(ElementName = "foundation_type", IsNullable = true)]
        public HouseFoundationTypeEnum? FoundationType { get; set; } //Идентификатор типа фундамента

        [XmlElement(ElementName = "floor_type", IsNullable = true)]
        public HouseFloorType988Enum? FloorType { get; set; } //Идентификатор типа перекрытий

        [XmlElement(ElementName = "wall_material", IsNullable = true)]
        public HouseWallMaterial988Enum? WallMaterial { get; set; } //Идентификатор материала несущих стен

        [XmlElement(ElementName = "area_basement", IsNullable = true)]
        public double? AreaBasement { get; set; } //Площадь подвала по полу, кв.м

        [XmlElement(ElementName = "chute_type", IsNullable = true)]
        public HouseChuteTypeEnum? ChuteType { get; set; } //Идентификатор типа мусоропровода

        [XmlElement(ElementName = "chute_count", IsNullable = true)]
        public int? ChuteCount { get; set; } //Количество мусоропроводов, ед.

        [XmlElement(ElementName = "electrical_type", IsNullable = true)]
        public HouseElectricalTypeEnum? ElectricalType { get; set; } //Идентификатор типа системы электроснабжения

        [XmlElement(ElementName = "electrical_entries_count", IsNullable = true)]
        public int? ElectricalEntriesCount { get; set; } //Количество вводов в МКД, ед.

        [XmlElement(ElementName = "heating_type", IsNullable = true)]
        public HouseHeatingTypeEnum? HeatingType { get; set; } //Идентификатор типа системы теплоснабжения

        [XmlElement(ElementName = "hot_water_type", IsNullable = true)]
        public HouseHotWaterTypeEnum? HotWaterType { get; set; } //Идентификатор типа системы горячего водоснабжения

        [XmlElement(ElementName = "cold_water_type", IsNullable = true)]
        public HouseColdWaterTypeEnum? ColdWaterType { get; set; } //Идентификатор типа системы холодного водоснабжения

        [XmlElement(ElementName = "sewerage_type", IsNullable = true)]
        public HouseSewerageTypeEnum? SewerageType { get; set; } //Идентификатор типа системы водоотведения

        [XmlElement(ElementName = "sewerage_cesspools_volume", IsNullable = true)]
        public double? SewerageSesspoolsVolume { get; set; } //Объем выгребных ям, куб.м

        [XmlElement(ElementName = "gas_type", IsNullable = true)]
        public HouseGasTypeEnum? GasType { get; set; } //Идентификатор типа системы газоснабжения

        [XmlElement(ElementName = "ventilation_type", IsNullable = true)]
        public HouseVentilationTypeEnum? VentilationType { get; set; } //Идентификатор типа системы вентиляции

        [XmlElement(ElementName = "firefighting_type", IsNullable = true)]
        public HouseFirefightingTypeEnum? FirefightingType { get; set; } //Идентификатор типа системы пожаротушения

        [XmlElement(ElementName = "drainage_type", IsNullable = true)]
        public HouseDrainageTypeEnum? DrainageType { get; set; } //Идентификатор типа системы пожаротушения

        [XmlArray(ElementName = "cadastral_numbers", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseCadastralNumber> CadastralNumbers { get; set; } //Кадастровый номер

        [XmlArray(ElementName = "facades", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseFacade> Facades { get; set; } //Фасад

        [XmlArray(ElementName = "roofs", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseRoof> Roofs { get; set; } //Крыша

        [XmlArray(ElementName = "additional_equipments", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseAdditionalEquipment> AdditionalEquipments { get; set; } //Иное оборудование/конструктивный элемент

        [XmlArray(ElementName = "metering_devices", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseMeteringDevice> MeteringDevices { get; set; } //Общедомовые приборы учета

        [XmlArray(ElementName = "lifts", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseLift> Lifts { get; set; } //Лифт

        [XmlElement(ElementName = "management_contract", IsNullable = true)]
        public HouseManagementContract ManagementContract { get; set; } //Управление

        [XmlArray(ElementName = "services", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseService> Services { get; set; } //Работы (услуги) по содержанию

        [XmlArray(ElementName = "communal_services", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseCommunalService> CommunalServices { get; set; } //Коммунальные услуги

        [XmlArray(ElementName = "common_properties", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseCommonProperty> CommonProperties { get; set; } //Общее имущество

        [XmlElement(ElementName = "overhaul", IsNullable = true)]
        public HouseOverhaul Overhaul { get; set; } //Сведения о КР

        [XmlArray(ElementName = "common_meetings", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseCommonMeeting> CommonMeetings { get; set; } //Общие собрания собственников дома

        [XmlElement(ElementName = "report", IsNullable = true)]
        public HouseReport Report { get; set; } //Отчет по управлению

        [XmlElement(ElementName = "alarm_info", IsNullable = true)]
        public HouseAlarm AlarmInfo { get; set; } //Информация при признании дома аварийным. При признании дома аварийным необходимо заполнить информацию из Таблица 152, для отказа от аварийности дополнительно заполнить информацию из Таблица 154. Пока дом аварийный – редактирование информации по аварийности доступно, если есть отказ от аварийности, то редактирование информации недоступно. Для повторного признания дома аварийным необходимо изменить хотя бы одно поле из Таблица 152, включая изменение идентификатора файла. При условии, что одновременно заполнены данные двух таблиц, произойдет  признание дома аварийным и сразу же отказ от аварийности.
    }
}
