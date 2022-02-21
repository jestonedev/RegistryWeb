using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseLiftTypeEnum
    {
        [XmlEnum(Name = "1")]
        Passenger = 1, //Пассажирский

        [XmlEnum(Name = "2")]
        Cargo, //Грузовой

        [XmlEnum(Name = "3")]
        PassengerCargo //Грузо-пассажирский
    }
}