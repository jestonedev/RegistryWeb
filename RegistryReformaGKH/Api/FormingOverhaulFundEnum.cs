using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum FormingOverhaulFundEnum
    {
        [XmlEnum(Name = "1")]
        SpecialOrg = 1, //На специальном счете организации

        [XmlEnum(Name = "2")]
        SpecialReg, //На специальном счете у регионального оператора

        [XmlEnum(Name = "3")]
        Reg, //На счете регионального оператора

        [XmlEnum(Name = "4")]
        Indefined //Не определен
    }
}