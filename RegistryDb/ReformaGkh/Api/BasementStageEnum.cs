using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public enum BasementStageEnum
    {
        NoData = 1, //Нет данных
        Missing, //отсутствует
        Exploited, //эксплуатируемый
        Unexploited, //неэксплуатируемый
    }
}
