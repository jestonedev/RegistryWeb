using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public class Chute
    {
        int ChuteCount { get; set; } //Количество мусоропроводов в доме
        string ChuteLastOverhaulDate { get; set; } //Год проведения последнего капитального ремонта мусоропроводов
    }
}
