using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public enum HouseTypeEnum
    {
        Dormitory = 1, //Общежитие
        IHO, //Individual housing object = Объект индивидуального жилищного строительства
        BRB, //Blocked residential building = Жилой дом блокированной застройки,
        AH //Apartment house = Многоквартирный дом
    }
}
