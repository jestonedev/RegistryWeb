using RegistryWeb.ViewOptions;
namespace RegistryWeb.ViewModel
{
    public class ListVM<F> where F :FilterOptions
    {
        public OrderOptions OrderOptions { get; set; }
        public PageOptions PageOptions { get; set; }
        public F FilterOptions { get; set; }
    }
}
