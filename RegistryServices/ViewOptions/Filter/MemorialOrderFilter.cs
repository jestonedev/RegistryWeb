using System;

namespace RegistryWeb.ViewOptions.Filter
{
    public class MemorialOrderFilter : FilterOptions
    {
        public string NumDocument { get; set; }
        public DateTime? DateDocument { get; set; }
        public decimal? Sum { get; set; }
        public string Kbk { get; set; }
        public string Okato { get; set; }

        public MemorialOrderFilter()
        {
        }
    }
}
