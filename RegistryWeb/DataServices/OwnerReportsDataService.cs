using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.DataServices
{
    public class OwnerReportsDataService
    {
        private readonly RegistryContext registryContext;
        public OwnerReportsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public Forma1VM GetForma1VM()
        {
            var viewModel = new Forma1VM();
            viewModel.Buildings =
                registryContext.Buildings
                .Join(registryContext.BuildingsOwnershipRightCurrent.Where(bb => bb.IdOwnershipRightType == 7),
                    b => b.IdBuilding,
                    borc => borc.IdBuilding,
                    (b, borc) => b)
                .Include(b => b.IdStreetNavigation);
            return viewModel;
        }

        public Forma2VM GetForma2VM()
        {
            var viewModel = new Forma2VM();
            var premises = registryContext.Premises
                .Join(registryContext.BuildingsOwnershipRightCurrent.Where(bb => bb.IdOwnershipRightType == 7),
                    p => p.IdBuilding,
                    borc => borc.IdBuilding,
                    (p, borc) => p)
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdPremisesTypeNavigation);
            viewModel.GroupPremises = premises.GroupBy(p => p.IdBuilding);
            var r = from p in premises
                    join oap in registryContext.OwnerActiveProcesses
                        on p.IdPremises equals oap.IdPremise
                    select new KeyValuePair<int,int>(p.IdPremises, oap.IdProcess);
            viewModel.PremisesIdOwnerProcesses = new Dictionary<int, int>(r);
            return viewModel;
        }
    }
}
