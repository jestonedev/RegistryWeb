using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Linq.Expressions;

namespace RegistryWeb.DataServices
{
    public class PremiseDataService
    {
        private readonly RegistryContext rc;

        public PremiseDataService(RegistryContext rc)
        {
            this.rc = rc;
        }

        public PremiseVM GetViewModel(int idPremise)
        {
            var viewModel = new PremiseVM();
            viewModel.Premise = GetQuery(idPremise).First();
            //viewModel.OwnershipRights = GetQueryOwnershipRights(idPremise);
            return viewModel;
        }

        public IQueryable<Premises> GetQuery(int idPremise)
        {
            return rc.Premises
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdPremisesTypeNavigation)
                .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
                .Include(p => p.IdPremisesTypeNavigation); //Тип помещения: квартира, комната, квартира с подселением
        }

        //public IQueryable<OwnershipRights> GetQueryOwnershipRights(int idPremise)
        //{
        //    return
        //        (from or in rc.OwnershipRights
        //         join oba in rc.OwnershipBuildingsAssoc
        //             on or.IdOwnershipRight equals oba.IdOwnershipRight
        //         join b in rc.Buildings
        //             on oba.IdBuilding equals b.IdBuilding
        //         where or.Deleted == 0 && oba.Deleted == 0 && b.Deleted == 0 && b.IdBuilding == idPremise
        //         select or)
        //        .Include(or => or.IdOwnershipRightTypeNavigation)
        //        .OrderBy(or => or.IdOwnershipRight);
        //}
    }
}
