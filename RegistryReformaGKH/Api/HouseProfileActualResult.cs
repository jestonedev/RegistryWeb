using System;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseProfileActualResult
    {
        [XmlElement(ElementName = "house_id")]
        public int HouseId { get; set; } //Идентификатор дома

        [XmlElement(ElementName = "full_address", IsNullable = true)]
        public FullAddress FullAddress { get; set; } //Адрес дома

        [XmlElement(ElementName = "stage")]
        public HouseStageEnum Stage { get; set; } //Идентификатор стадии жизненного цикла дома

        [XmlElement(ElementName = "inn", IsNullable = true)]
        public string Inn { get; set; } //ИНН организации, в управлении которой находится дом

        [XmlElement(ElementName = "last_update")]
        public DateTime LastUpdate { get; set; } //Последнее изменение анкеты

        [XmlElement(ElementName = "house_profile_data", IsNullable = true)]
        public HouseProfileData988 HouseProfileData { get; set; } //Массив по данным

        [XmlElement(ElementName = "files_info", IsNullable = true)]
        public FileInfo FilesInfo { get; set; } //Массив файлов анкеты многоквартирного дома
    }
}
