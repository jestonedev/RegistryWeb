namespace RegistryWeb.ViewOptions.Filter
{
    public class TenancyProcessesFilter : FilterAddressOptions
    {
        public int? IdProcess { get; set; }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }

        public bool IsModalEmpty()
        {
            return true;
        }
    }
}
