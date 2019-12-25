using System;

namespace RegistryWeb.Models.Api
{
    public class HouseMeteringDevice
    {
        //id
        int Id { get; set; }
        //Communal_resource_type
        HouseCommunalServiceTypeEnum CommunalResourceType { get; set; }
        //availability
        MeteringDeviceAvailabilityEnum Availability { get; set; }
        //meter_type
        HouseMeterTypeEnum MeterType { get; set; }
        //Unit_of_measurement
        UnitOfMeasureEnum UnitOfMeasurement { get; set; }
        //Commissioning_date
        DateTime CommissioningDate { get; set; }
        //Calibration_date
        DateTime CalibrationDate { get; set; }
        //Is_default
        bool IsDefault { get; set; }
    }
}