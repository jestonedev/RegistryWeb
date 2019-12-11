using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public enum HotWaterSystemNameEnum
    {
        NoData = 1, //Нет данных
        Missing, //отсутствует
        CentralizedOpen, //централизованная открытая
        CentralizedClosed, //централизованная закрытая
        Apartment, //поквартирная
        Autonomous //автономная
    }
}
