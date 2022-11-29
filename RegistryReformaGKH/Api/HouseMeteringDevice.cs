using System;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseMeteringDevice
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "communal_resource_type", IsNullable = true)]
        public HouseCommunalServiceTypeEnum? CommunalResourceType { get; set; }

        [XmlElement(ElementName = "availability", IsNullable = true)]
        public MeteringDeviceAvailabilityEnum? Availability { get; set; }

        [XmlElement(ElementName = "meter_type", IsNullable = true)]
        public HouseMeterTypeEnum? MeterType { get; set; }

        [XmlElement(ElementName = "unit_of_measurement", IsNullable = true)]
        public UnitOfMeasureEnum? UnitOfMeasurement { get; set; }

        [XmlElement(ElementName = "commissioning_date", IsNullable = true)]
        public DateTime? CommissioningDate { get; set; }

        [XmlElement(ElementName = "calibration_date", IsNullable = true)]
        public DateTime? CalibrationDate { get; set; }

        [XmlElement(ElementName = "is_default", IsNullable = true)]
        public bool? IsDefault { get; set; }
    }
}