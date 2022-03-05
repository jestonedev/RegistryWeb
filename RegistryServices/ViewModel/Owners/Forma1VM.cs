using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Linq;

namespace RegistryServices.ViewModel.Owners
{
    public class Forma1VM : ListVM<BuildingsFilter>
    {
        public IQueryable<Building> Buildings { get; set; }
    }
}
