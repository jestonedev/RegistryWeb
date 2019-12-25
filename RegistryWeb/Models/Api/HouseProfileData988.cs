using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Api
{
    public class HouseProfileData988
    {
        //area_total
        double AreaTotal { get; set; }  //Общая площадь дома, кв.м
        //area_residental
        double AreaResidental { get; set; } //Общая площадь дома, в т.ч. жилых помещений, кв.м
        //area_non_residental
        double AreaNonResidental { get; set; } //Общая площадь дома, в т.ч. нежилых помещений, кв.м
        //area_common_property
        double AreaCommonProperty { get; set; } //Общая площадь дома, в т.ч. помещений, входящих в состав общего имущества, кв.м.
        //is_alarm
        bool IsAlarm { get; set; } //Признак аварийности. Возможные значения: true (да, дом аварийный), false (нет, дом не аварийный). Значение по умолчанию: «Нет». Информация по аварийности или при отказе от аварийности заполняется в теге «alarm_info».
        //method_of_forming_overhaul_fund
        FormingOverhaulFundEnum MethodOfFormingOverhaulFund { get; set; } //Способ формирования фонда капитального ремонта 
        //exploitation_start_year
        int ExploitationStartYear { get; set; } //Год ввода в эксплуатацию (Формат: ‘2015’)
        //project_type
        string ProjectType { get; set; } //Серия, тип постройки здания
        //built_year
        int BuiltYear { get; set; } //Год постройки. (Формат: ‘2015’)
        //house_type
        HouseType988Enum HouseType { get; set; } //Тип дома (описано в Таблица 157)
        //floor_count_max
        int FloorCountMax { get; set; } //Количество этажей, наибольшее, ед.
        //floor_count_min
        int FloorCountMin { get; set; } //Количество этажей, наименьшее, ед.
        //entrance_count
        int EntranceCount { get; set; } //Количество подъездов
        //elevators_count
        int ElevatorsCount { get; set; } //Количество лифтов
        //flats_count
        int FlatsCount { get; set; } //Количество квартир
        //living_quarters_count
        int LivingQuartersCount { get; set; } //Количество помещений, в том числе, жилых, ед.
        //not_living_quarters_count
        int NotLivingQuartersCount { get; set; } //Количество помещений, в том числе, нежилых, ед.
        //area_land
        double AreaLand { get; set; } //Площадь земельного участка, входящего в состав общего имущества в многоквартирном доме, кв.м
        //parking_square
        double ParkingSquare { get; set; } //Площадь парковки в границах земельного участка, кв.м
        //energy_efficiency
        HouseEnergyEfficiencyClass988Enum EnergyEfficiency { get; set; } //Класс энергетической эффективности
        //additional_info
        string AdditionalInfo { get; set; } //Дополнительная информация
        //has_playground
        bool HasPlayground { get; set; } //Элементы благоустройства  (детская площадка).  Возможные значения true (да, детская площадка имеется), false (нет, не имеется).
        //has_sportsground
        bool HasSportsground { get; set; } //Элементы благоустройства (спортивная площадка). Возможные значения true (да, спортивная площадка имеется) , false (нет, не имеется).
        //other_beautification
        string OtherBeautification { get; set; } //Элементы благоустройства  (другое)
        //foundation_type
        HouseFoundationTypeEnum FoundationType { get; set; }
        //floor_type
        HouseFloorType988Enum FloorType { get; set; } //Идентификатор типа перекрытий
        //wall_material
        HouseWallMaterial988Enum WallMaterial { get; set; } //Идентификатор материала несущих стен
        //area_basement
        double AreaBasement { get; set; } //Площадь подвала по полу, кв.м
        //chute_type
        HouseChuteTypeEnum ChuteType { get; set; } //Идентификатор типа мусоропровода 
        //chute_count
        int ChuteCount { get; set; } //Количество мусоропроводов, ед.
        //electrical_type
        HouseElectricalTypeEnum ElectricalType { get; set; } //Идентификатор типа системы электроснабжения
        //electrical_entries_count
        int ElectricalEntriesCount { get; set; } //Количество вводов в МКД, ед.
        //heating_type
        HouseHeatingTypeEnum HeatingType { get; set; } //Идентификатор типа системы теплоснабжения
        //hot_water_type
        HouseHotWaterTypeEnum HotWaterType { get; set; } //Идентификатор типа системы горячего водоснабжения
        //cold_water_type
        HouseColdWaterTypeEnum ColdWaterType { get; set; } //Идентификатор типа системы холодного водоснабжения
        //sewerage_type
        HouseSewerageTypeEnum SewerageType { get; set; } //Идентификатор типа системы водоотведения
        //sewerage_cesspools_volume
        double SewerageSesspoolsVolume { get; set; } //Объем выгребных ям, куб.м
        //gas_type
        HouseGasTypeEnum GasType { get; set; } //Идентификатор типа системы газоснабжения 
        //ventilation_type
        HouseVentilationTypeEnum VentilationType { get; set; } //Идентификатор типа системы вентиляции 
        //firefighting_type
        HouseFirefightingTypeEnum FirefightingType { get; set; } //Идентификатор типа системы пожаротушения
        //drainage_type
        HouseDrainageTypeEnum DrainageType { get; set; } //Идентификатор типа системы пожаротушения
        //cadastral_numbers
        List<HouseCadastralNumber> CadastralNumbers { get; set; } //Кадастровый номер
        //facades
        List<HouseFacade> Facades { get; set; } //Фасад
        //roofs
        List<HouseRoof> Roofs { get; set; } //Крыша
        //additional_equipments
        List<HouseAdditionalEquipment> AdditionalEquipments { get; set; } //Иное оборудование/конструктивный элемент
        //metering_devices
        List<HouseMeteringDevice> MeteringDevices { get; set; } //Общедомовые приборы учета
        //lifts
        List<HouseLift> Lifts { get; set; } //Лифт
        //management_contract
        HouseManagementContract ManagementContract { get; set; } //Управление
        //services
        List<HouseService> Services { get; set; } //Работы (услуги) по содержанию
        //communal_services
        List<HouseCommunalService> CommunalServices { get; set; } //Коммунальные услуги
        //common_properties
        List<HouseCommonProperty> CommonProperties { get; set; } //Общее имущество
        //overhaul
        HouseOverhaul Overhaul { get; set; } //Сведения о КР
        //common_meetings
        List<HouseCommonMeeting> CommonMeetings { get; set; } //Общие собрания собственников дома
        //report
        HouseReport Report { get; set; } //Отчет по управлению 
        //alarm_info
        HouseAlarm AlarmInfo { get; set; } //Информация при признании дома аварийным. При признании дома аварийным необходимо заполнить информацию из Таблица 152, для отказа от аварийности дополнительно заполнить информацию из Таблица 154. Пока дом аварийный – редактирование информации по аварийности доступно, если есть отказ от аварийности, то редактирование информации недоступно. Для повторного признания дома аварийным необходимо изменить хотя бы одно поле из Таблица 152, включая изменение идентификатора файла. При условии, что одновременно заполнены данные двух таблиц, произойдет  признание дома аварийным и сразу же отказ от аварийности.
    }
}
