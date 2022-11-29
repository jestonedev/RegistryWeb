using RegistryWeb.Enums;

namespace RegistryWeb.ViewOptions.Filter
{
    public class ReestrEmergencyPremisesFilter : FilterAddressOptions
    {
        public ProcessOwnershipTypeEnum ProcessOwnershipType { get; set; }
        public string Persons { get; set; }

        public bool IsEmpty()
        {
            return IsAddressEmpty() &&
                ProcessOwnershipType == ProcessOwnershipTypeEnum.All &&
                string.IsNullOrWhiteSpace(Persons);
        }
    }
}
