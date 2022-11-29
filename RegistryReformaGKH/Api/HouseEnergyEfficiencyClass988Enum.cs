using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseEnergyEfficiencyClass988Enum
    {
        [XmlEnum(Name = "1")]
        NotAssigned = 1, //Не присвоен

        [XmlEnum(Name = "2")]
        A,

        [XmlEnum(Name = "3")]
        BPlusPlus,

        [XmlEnum(Name = "4")]
        BPlus,

        [XmlEnum(Name = "5")]
        C,

        [XmlEnum(Name = "6")]
        D,

        [XmlEnum(Name = "7")]
        E,

        [XmlEnum(Name = "8")]
        B,

        [XmlEnum(Name = "9")]
        APlusPlus,

        [XmlEnum(Name = "10")]
        APlus,

        [XmlEnum(Name = "11")]
        F,

        [XmlEnum(Name = "12")]
        G
    }
}