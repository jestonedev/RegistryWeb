using RegistryWeb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
namespace RegistryWeb.ViewModel
{
    public class BuildingsVM : ListVM<BuildingsFilter>
    {
        public IEnumerable<Building> Buildings { get; set; }
        public IEnumerable<ObjectState> ObjectStates { get; set; }
    }
}
