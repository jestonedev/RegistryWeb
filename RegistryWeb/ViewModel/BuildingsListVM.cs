using RegistryWeb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
namespace RegistryWeb.ViewModel
{
    public class BuildingsListVM : ListVM<BuildingsListFilter>
    {
        public IEnumerable<Buildings> Buildings { get; set; }
        public IEnumerable<ObjectStates> ObjectStates { get; set; }
    }
}
