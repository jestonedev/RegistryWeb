using RegistryWeb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
namespace RegistryWeb.ViewModel
{
    public class BuildingsVM : ListVM<BuildingsFilter>
    {
        public List<Building> Buildings { get; set; }
        public Dictionary<int, bool> IsMunicipalDictionary { get; set; }
    }
}
