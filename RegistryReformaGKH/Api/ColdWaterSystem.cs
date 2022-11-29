namespace RegistryReformaGKH.Api
{
    public class ColdWaterSystem
    {
        ColdWaterSystemNameEnum SystemName { get; set; } //Тип
        double SystemLenght { get; set; } //Длина трубопроводов системы холодного водоснабжения, м
        string LastOverhaulDate { get; set; } //Год проведения последнего капитального ремонта системы холодного водоснабжения
        int InputPointsCount { get; set; } //Количество точек ввода холодной воды
        int MeteringDevicesCount { get; set; } // Количество общедомовых приборов учета холодной воды
        ProvisioningEnum Provisioning { get; set; } //Отпуск холодной воды производится
    }
}