using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public enum HouseWallMaterialEnum
    {
        NoData = 1, //Нет данных
        Stone, //Каменные, кирпичные
        Panel, //Панельные
        Blocky, //Блочные
        Mixed, //Смешанные
        Monolithic, //Монолитные
        Wooden, //Деревянные
        Other //Прочие
    }
}
