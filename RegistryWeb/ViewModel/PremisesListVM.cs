using RegistryDb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.ViewModel
{
    public class PremisesListVM : ListVM<PremisesListFilter>
    {
        public IEnumerable<Premise> Premises { get; set; }
        public IEnumerable<PremisesType> PremisesTypes { get; set; }
        public IEnumerable<ObjectState> ObjectStates { get; set; }
        public IEnumerable<FundType> FundTypes { get; set; }
    }
}
