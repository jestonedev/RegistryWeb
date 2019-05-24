using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.DataServices
{
    public class BuildingDataService
    {
        private readonly RegistryContext rc;

        public BuildingDataService(RegistryContext rc)
        {
            this.rc = rc;
        }

        public BuildingVM GetViewModel(int idBuilding)
        {
            var viewModel = new BuildingVM();
            viewModel.Building = GetQuery(idBuilding).First();
            viewModel.OwnershipRights = GetQueryOwnershipRights(idBuilding);
            return viewModel;
        }

        public IQueryable<Buildings> GetQuery(int idBuilding)
        {
            return rc.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)
                .Include(b => b.IdStructureTypeNavigation)
                .Where(b => b.IdBuilding == idBuilding);
        }

        public IQueryable<OwnershipRights> GetQueryOwnershipRights(int idBuilding)
        {
            return 
                (from or in rc.OwnershipRights
                join oba in rc.OwnershipBuildingsAssoc
                    on or.IdOwnershipRight equals oba.IdOwnershipRight
                join b in rc.Buildings
                    on oba.IdBuilding equals b.IdBuilding
                where b.IdBuilding == idBuilding
                select or)
                .Include(or => or.IdOwnershipRightTypeNavigation)
                .OrderBy(or => or.IdOwnershipRight);
        }
    }
}
