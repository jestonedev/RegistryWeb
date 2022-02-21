﻿using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum HouseElectricalTypeEnum
    {
        [XmlEnum(Name = "1")]
        Missing = 1, //Отсутствует

        [XmlEnum(Name = "2")]
        Cental, //Центральное

        [XmlEnum(Name = "3")]
        Combined //Комбинированное
    }
}