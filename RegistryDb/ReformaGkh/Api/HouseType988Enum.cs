using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseType988Enum
    {
        [XmlEnum(Name = "1")]
        AH = 1,//Apartment house = Многоквартирный дом

        [XmlEnum(Name = "2")]
        BRB, //Blocked residential building = Жилой дом блокированной застройки

        [XmlEnum(Name = "3")]
        SHF, //Specialized Housing Fund = Специализированный жилищный фонд

        [XmlEnum(Name = "4")]
        House //Жилой дом (индивидуально-определенное здание) (это значение не доступно для выбора, по причине исключения из справочника)
    }
}
