using System;
namespace RegistryWeb.Models.Api
{
    public class HouseProfileData
    {
        double AreaTotal { get; set; }  //Общая площадь помещений
        double AreaResidental { get; set; } //В том числе жилых помещений
        double AreaNonResidental { get; set; } //Общая площадь нежилых помещений
        string CadastralNumber { get; set; } //Кадастровый номер
        string ExploitationStartYear { get; set; } //Год ввода в эксплуатацию
        string ProjectType { get; set; } //Серия, тип проекта
        string LocationDescription { get; set; } //Описание местоположения
        string IndividualName { get; set; } //Индивидуальное наименование дома
        HouseTypeEnum HouseType { get; set; } //Идентификатор типа жилого дома
        HouseWallMaterialEnum WallMaterial { get; set; } //Идентификатор материала несущих стен
        HouseFloorTypeEnum FloorType { get; set; } //Идентификатор типа перекрытий
        int StoreysCount { get; set; } //Этажность
        int EntranceCount { get; set; } //Количество подъездов
        int ElevatorsCount { get; set; } //Количество лифтов
        double AreaPrivate { get; set; }    //Частная
        double AreaMunicipal { get; set; } //Муниципальная
        double AreaNational { get; set; } //Государственная
        double AreaLand { get; set; } //Площадь участка, м2
        double AreaTerritory { get; set; } //Площадь придомовой территории, м2
        string InventoryNumber { get; set; } //Инвентарный номер
        int FlatsCount { get; set; } //Количество квартир
        string ResidentsCount { get; set; } //Количество жителей
        int AccountsCount { get; set; } //Количество лицевых счетов
        string ConstuctionFeatures { get; set; } //Конструктивные особенности дома
        double ThermalActualExpense { get; set; } //фактический удельный расход, Вт/М3Сград
        double ThermalNormativeExpense { get; set; } //нормативный удельный расход, Вт/М3Сград
        HouseEnergyEfficiencyClassEnum EnergyEfficiency { get; set; } //Идентификатор класса энергоэффективности
        DateTime EnergyAuditDate { get; set; } //Дата проведения энергетического аудита
        DateTime PrivatizationStartDate { get; set; } //Дата начала приватизации
        double DeteriorationTotal { get; set; } //Общая степень износа
        double DeteriorationFoundation { get; set; } //Степень износа фундамента
        double DeteriorationBearingWalls { get; set; } //Степень износа несущих стен
        double DeteriorationFloor { get; set; } //Степень износа перекрытий
        Facade Facade { get; set; } //Фасад
        Roof Roof { get; set; } //Кровля
        Basement Basement { get; set; } //Подвал
        CommonSpace CommonSpace { get; set; } //Помещения общего пользования
        Chute Chute { get; set; } //Мусоропроводы
        HeatingSystem HeatingSystem { get; set; } //Система отопления
        HotWaterSystem HotWaterSystem { get; set; } //Система горячего водоснабжения
        ColdWaterSystem ColdWaterSystem { get; set; } //Система холодного водоснабжения
        SewerageSystem SewerageSystem { get; set; } //Система водоотведения(канализация)
        ElectricitySystem ElectricitySystem { get; set; } //Система электроснабжения
        GasSystem GasSystem { get; set; } //Система газоснабжения
        Lift Lifts { get; set; } //Лифт
        ManagementContract ManagementContract { get; set; } //Управление
        Provider HeatingProvider { get; set; }//Поставщик отопления
        Provider ElectricityProvider  { get; set; } //Поставщик электричества
        Provider GasProvider  { get; set; } //Поставщик газа
        Provider HotWaterProvider { get; set; } //Поставщик горячей воды
        Provider ColdWaterProvider  { get; set; } //Поставщик холодной воды
        Provider DrainageProvider { get; set; } //Поставщик водоотведения
        Finance Finance { get; set; } //Финансы
    }
}
