namespace RegistryReformaGKH.Api
{
    public class HeatingSystem
    {
        HeatingSystemNameEnum SystemName { get; set; } //Идентификатор типа
        int Elevators { get; set; } //Количество элеваторных узлов системы отопления
        double SystemLength { get; set; } //Длина трубопроводов системы отопления, м
        string LastOverhaulDate { get; set; } //Год проведения последнего капитального ремонта системы отопления
        int InputPointsCount { get; set; } //Количество точек ввода отопления
        int ControlNodesCount { get; set; } //Количество узлов управления отоплением
        int MeteringDevicesCount { get; set; } //Количество общедомовых приборов учета отопления
        ProvisioningEnum Provisioning { get; set; } //Идентификатор отпуска отопления
    }
}