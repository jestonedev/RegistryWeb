using RegistryDb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Linq;

namespace RegistryWeb.ViewModel
{
    public class Forma1VM : ListVM<BuildingsFilter>
    {
        public IQueryable<Building> Buildings { get; set; }
    }
}
