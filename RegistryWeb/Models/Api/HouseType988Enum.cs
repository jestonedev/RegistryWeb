using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public enum HouseType988Enum
    {
        AH = 1,//Apartment house = Многоквартирный дом
        BRB, //Blocked residential building = Жилой дом блокированной застройки
        SHF, //Specialized Housing Fund = Специализированный жилищный фонд
        House //Жилой дом (индивидуально-определенное здание) (это значение не доступно для выбора, по причине исключения из справочника)
    }
}
