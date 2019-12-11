using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public enum ProvisioningEnum
    {
        NoData = 1, //нет данных
        NormOrPU, //по нормативам или квартирным ПУ
        CommonPU //по показаниям общедомовых ПУ
    }
}
