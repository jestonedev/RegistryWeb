namespace RegistryReformaGKH.Api
{
    public class ElectricitySystem
    {
        ElectricitySystemNameEnum SystemName { get; set; } //Система электроснабжения(Возможные значения в Таблица 92)
        double SystemLength { get; set; } //Длина сетей в местах общего пользования, м
        string LastOverhaulDate { get; set; } //проведения последнего капремонта системы электроснабжения
        int InputPointsCount { get; set; } //Количество точек ввода электричества
        int MeteringDevicesCount { get; set; } //Количество общедомовых приборов учета электричества
        ProvisioningEnum Provisioning { get; set; } //Отпуск электричества производится
    }
}