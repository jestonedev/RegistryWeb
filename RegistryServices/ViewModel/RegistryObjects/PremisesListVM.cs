using RegistryDb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryDb.Models.Entities.RegistryObjects.Common.Funds;
using RegistryDb.Models.Entities.RegistryObjects.Common;

namespace RegistryServices.ViewModel.RegistryObjects
{
    public class PremisesListVM : ListVM<PremisesListFilter>
    {
        public IEnumerable<Premise> Premises { get; set; }
        public IEnumerable<PremisesType> PremisesTypes { get; set; }
        public IEnumerable<ObjectState> ObjectStates { get; set; }
        public IEnumerable<FundType> FundTypes { get; set; }
    }
}
