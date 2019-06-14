using RegistryWeb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
namespace RegistryWeb.ViewModel
{
    public class BuildingsListVM : ListVM<BuildingsListFilter>
    {
        public IEnumerable<Building> Buildings { get; set; }
        public IEnumerable<ObjectState> ObjectStates { get; set; }
    }
}
