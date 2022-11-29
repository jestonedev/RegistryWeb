using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseRoofingTypeEnum
    {
        [XmlEnum(Name = "1")]
        Slate = 1, //Из волнистых и полуволнистых асбестоцементных листов (шиферная)

        [XmlEnum(Name = "2")]
        Steel, //Из оцинкованной стали

        [XmlEnum(Name = "3")]
        MetalTile, //Из металлочерепицы

        [XmlEnum(Name = "4")]
        ProfiledFlooring, //Из профилированного настила

        [XmlEnum(Name = "5")]
        RollMaterial, //Из рулонных материалов

        [XmlEnum(Name = "6")]
        SoftRoof, //Мягкая (наплавляемая) крыша

        [XmlEnum(Name = "7")]
        OtherMaterial, //Из иного материала

        [XmlEnum(Name = "8")]
        Ferroconcrete //Безрулонная железобетонная крыша
    }
}