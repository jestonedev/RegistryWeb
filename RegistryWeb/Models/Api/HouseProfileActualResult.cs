using System;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseProfileActualResult
    {
        [XmlElement(ElementName = "house_id")]
        public int HouseId { get; set; }
        [XmlElement(ElementName = "full_address")]
        public FullAddress FullAddress { get; set; }
        [XmlElement(ElementName = "stage")]
        public HouseStageEnum Stage { get; set; }
        [XmlElement(ElementName = "inn")]
        public string Inn { get; set; }
        [XmlElement(ElementName = "last_update")]
        public DateTime LastUpdate { get; set; }
        [XmlElement(ElementName = "house_profile_data")]
        [XmlIgnore]
        public HouseProfileData988 HouseProfileData { get; set; }
        [XmlElement(ElementName = "files_info")]
        public FileInfo FilesInfo { get; set; }
    }
}
