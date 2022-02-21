using Microsoft.EntityFrameworkCore;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.Enums;
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

        public Forma1VM GetForma1VM(BuildingsFilter filterOptions)
        {
            var viewModel = new Forma1VM
            {
                FilterOptions = filterOptions ?? new BuildingsFilter()
            };
            viewModel.Buildings =
                registryContext.Buildings
                .Join(registryContext.BuildingsOwnershipRightCurrent.Where(bb => bb.IdOwnershipRightType == 7),
                    b => b.IdBuilding,
                    borc => borc.IdBuilding,
                    (b, borc) => b)
                .Include(b => b.IdStreetNavigation);
            if (viewModel.FilterOptions.Address.AddressType == AddressTypes.Street)
            {
                viewModel.Buildings = viewModel.Buildings.Where(b => b.IdStreet == viewModel.FilterOptions.Address.Id);
            }
            if (viewModel.FilterOptions.Address.AddressType == AddressTypes.Building)
            {
                var idBuilding = 0;
                if (int.TryParse(viewModel.FilterOptions.Address.Id, out idBuilding))
                {
                    viewModel.Buildings = viewModel.Buildings.Where(b => b.IdBuilding == idBuilding);
                }
            }
            return viewModel;
        }

        public Forma2VM GetForma2VM(PremisesListFilter filterOptions)
        {
            var viewModel = new Forma2VM
            {
                FilterOptions = filterOptions ?? new PremisesListFilter()
            };
            
            var premises = registryContext.Premises
                .Join(registryContext.BuildingsOwnershipRightCurrent.Where(bb => bb.IdOwnershipRightType == 7),
                    p => p.IdBuilding,
                    borc => borc.IdBuilding,
                    (p, borc) => p)
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdPremisesTypeNavigation);

            var premisesList = premises.Where(p => true);

            if (viewModel.FilterOptions.Address.AddressType == AddressTypes.Street)
            {
                premisesList = premises.Where(p => p.IdBuildingNavigation.IdStreet == viewModel.FilterOptions.Address.Id);
            }
            if (viewModel.FilterOptions.Address.AddressType == AddressTypes.Building)
            {
                var idBuilding = 0;
                if (int.TryParse(viewModel.FilterOptions.Address.Id, out idBuilding))
                {
                    premisesList = premises.Where(b => b.IdBuilding == idBuilding);
                }
            }

            viewModel.GroupPremises = premisesList.GroupBy(p => p.IdBuilding);
            var r = from p in premisesList
                    join oap in registryContext.OwnerActiveProcesses
                        on p.IdPremises equals oap.IdPremise
                    select new KeyValuePair<int,int>(p.IdPremises, oap.IdProcess);
            viewModel.PremisesIdOwnerProcesses = new Dictionary<int, int>(r);
            return viewModel;
        }
    }
}
