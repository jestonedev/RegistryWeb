using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    [DataContract]
    public enum HouseWallMaterial988Enum
    {
        [EnumMember(Value = "1")]
        Stone = 1, //Каменные, кирпичные
        [EnumMember(Value = "2")]
        Panel, //Панельные
        [EnumMember(Value = "3")]
        Blocky, //Блочные
        [EnumMember(Value = "4")]
        Mixed, //Смешанные
        [EnumMember(Value = "5")]
        Wooden, //Деревянные
        [EnumMember(Value = "6")]
        Monolithic, //Монолитные
        [EnumMember(Value = "7")]
        Other, //Иные
        [EnumMember(Value = "8")]
        NotFilled, //Не заполнено
        [EnumMember(Value = "9")]
        ClayditeConcreteBlocks, //Керамзитобетон (блоки)
        [EnumMember(Value = "10")]
        FerroConcrete, //Железобетон
        [EnumMember(Value = "11")]
        ClayditeConcrete, //Керамзитобетон
        [EnumMember(Value = "12")]
        FerroConcretePanel, //Железобетонная панель
        [EnumMember(Value = "13")]
        ClayditeConcreteOneLayerPanel, //Керамзитобетонная 1-слойная панель
        [EnumMember(Value = "14")]
        FerroConcreteThreeLayerPanel, //Ж/б 3-х слойная панель с утеплителем
        [EnumMember(Value = "15")]
        SlagConcreteBlocks, //Шлакобетон (блоки)
        [EnumMember(Value = "16")]
        SlagClayditeConcreteOneLayerPanel //Шлакокерамзитобетонная 1-слойная панель
    }
}
