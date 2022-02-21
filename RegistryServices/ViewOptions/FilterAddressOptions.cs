using RegistryWeb.Enums;
using RegistryWeb.ViewModel;

namespace RegistryWeb.ViewOptions
{
    public class FilterAddressOptions : FilterOptions
    {
        public FilterAddressOptions()
        {
            Address = new Address();
        }

        public Address Address { get; set; }

        public bool IsAddressEmpty()
        {
            return Address == null || Address.AddressType == AddressTypes.None ||
                string.IsNullOrWhiteSpace(Address.Text);
        }
    }
}
