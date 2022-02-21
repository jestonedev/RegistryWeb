using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public enum HeatingSystemNameEnum
    {
        NoData = 1, //Нет данных
        Missing, //отсутствует
        Central, //центральное
        Autonomous, //автономное
        Apartment, //поквартирное
        Stove //печное
    }
}
