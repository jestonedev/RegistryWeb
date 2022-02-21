using System;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class ReportingPeriod
    {
        [XmlElement(ElementName ="id")]
        public int Id { get; set; }
        [XmlElement(ElementName = "date_start")]
        public DateTime DateStart { get; set; }
        [XmlElement(ElementName = "date_end")]
        public DateTime DateEnd { get; set; }
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "state")]
        public ReportingPeriodStateEnum State { get; set; }
        [XmlElement(ElementName = "is_988")]
        public bool Is988 { get; set; }
    }
}
