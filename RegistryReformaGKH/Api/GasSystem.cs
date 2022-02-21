namespace RegistryReformaGKH.Api
{
    public class GasSystem
    {
        GasSystemNameEnum SystemName { get; set; } //Вид системы газоснабжения
        double SystemLength { get; set; } //Длина сетей соответствующих требованиям
        double SystemLengthNoRequirements { get; set; } //Длина сетей не соответствующих требованиям
        string LastOverhaulDate { get; set; } //Год проведения последнего капремонта системы газоснабжения
        int InputPointsCount { get; set; } //Количество точек ввода газа
        int MeteringDevicesCount { get; set; } //Количество общедомовых приборов учета газа
        ProvisioningEnum Provisioning { get; set; } // Отпуск газа производится
    }
}