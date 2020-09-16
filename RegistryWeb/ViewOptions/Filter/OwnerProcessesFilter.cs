namespace RegistryWeb.ViewOptions.Filter
{
    public class OwnerProcessesFilter : FilterAddressOptions
    {
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public int? IdOwnerType { get; set; }
        public int? IdProcessType { get; set; } //все/действующие/аннулированые
        public int? IdProcess { get; set; }

        public bool IsModalEmpty()
        {
            return IdStreet == null && House == null && PremisesNum == null &&
                (IdOwnerType == null || IdOwnerType.Value == 0) &&
                (IdProcess == null || IdProcess.Value == 0) &&
                (IdProcessType == null || IdProcessType.Value == 0);
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }
    }
}
