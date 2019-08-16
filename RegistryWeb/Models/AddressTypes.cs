using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public enum AddressTypes
    {
        None = 0,        // Неопределенный адрес
        Street = 1,      // Улица
        Building = 2,    // Здание
        Premise = 3,     // Помещение внутри здания
        SubPremise = 4   // Комната внутри помещения
    }
}
