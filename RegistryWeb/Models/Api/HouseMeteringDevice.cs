using System;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseMeteringDevice
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "Communal_resource_type", IsNullable = true)]
        public HouseCommunalServiceTypeEnum? CommunalResourceType { get; set; }

        [XmlElement(ElementName = "availability", IsNullable = true)]
        public MeteringDeviceAvailabilityEnum? Availability { get; set; }

        [XmlElement(ElementName = "meter_type", IsNullable = true)]
        public HouseMeterTypeEnum? MeterType { get; set; }

        [XmlElement(ElementName = "Unit_of_measurement", IsNullable = true)]
        public UnitOfMeasureEnum? UnitOfMeasurement { get; set; }

        [XmlElement(ElementName = "Commissioning_date", IsNullable = true)]
        public DateTime? CommissioningDate { get; set; }

        [XmlElement(ElementName = "Calibration_date", IsNullable = true)]
        public DateTime? CalibrationDate { get; set; }

        [XmlElement(ElementName = "Is_default", IsNullable = true)]
        public bool? IsDefault { get; set; }
    }
}