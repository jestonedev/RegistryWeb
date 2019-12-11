using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public enum HouseEnergyEfficiencyClassEnum
    {
        NoData = 1, //Нет данных
        NotAssigned, //Не присвоен
        A,
        B,
        BPlus,
        BPlusPlus,
        C,
        D,
        E
    }
}
