using RegistryDb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class Forma2VM : ListVM<PremisesListFilter>
    {
        public IQueryable<IGrouping<int, Premise>> GroupPremises { get; set; }
        public Dictionary<int, int> PremisesIdOwnerProcesses { get; set; }
    }
}
