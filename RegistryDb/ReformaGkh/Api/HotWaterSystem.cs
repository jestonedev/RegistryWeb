using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public class HotWaterSystem
    {
        HotWaterSystemNameEnum SystemName { get; set; } //Тип
        double SystemLength { get; set; } //Длина трубопроводов системы горячего водоснабжения, м
        string LastOverhaulDate { get; set; } //Год проведения последнего капитального ремонта системы горячего водоснабжения
        int InputPointsCount { get; set; } //Количество точек ввода горячей воды
        int ControlNodesCount { get; set; } //Количество узлов управления поставкой горячей воды
        int MeteringDevicesCount { get; set; } //Количество общедомовых приборов учета горячей воды
        ProvisioningEnum Provisioning { get; set; } //Отпуск горячей воды производится
    }
}
