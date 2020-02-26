namespace RegistryWeb.ViewOptions.Filter
{
    public class OwnerProcessesFilter : FilterAddressOptions
    {
        public int? IdOwnerType { get; set; }
        public int IdProcessType { get; set; } //все/действующие/аннулированые
        public int? IdProcess { get; set; }

        public bool IsEmpty()
        {
            return IsAddressEmpty() &&
                (IdOwnerType == null || IdOwnerType.Value == 0) &&
                (IdProcess == null || IdProcess.Value == 0) &&
                IdProcessType == 0;
        }
    }
}
