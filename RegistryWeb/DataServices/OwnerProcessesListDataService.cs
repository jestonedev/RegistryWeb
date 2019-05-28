using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.DataServices
{
    public class OwnerProcessesListDataService : ListDataService<OwnerProcessesListVM, OwnerProcessesListFilter>
    {
        public OwnerProcessesListDataService(RegistryContext registryContext) : base(registryContext)
        {
        }

        public override OwnerProcessesListVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, OwnerProcessesListFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.OwnerTypes = registryContext.OwnerType;
            return viewModel;
        }

        public OwnerProcessesListVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            OwnerProcessesListFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.OwnerProcesses = GetQueryPage(query, viewModel.PageOptions);
            return viewModel;
        }

        public IQueryable<OwnerProcesses> GetQuery()
        {
            return registryContext.OwnerProcesses
                .Include(op => op.IdOwnerTypeNavigation);
        }

        private IQueryable<OwnerProcesses> GetQueryFilter(IQueryable<OwnerProcesses> query, OwnerProcessesListFilter filterOptions)
        {
            //if (!string.IsNullOrEmpty(filterOptions.Street))
            //{
            //    query = query.Where(p => p.IdBuildingNavigation.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(filterOptions.Street.ToLowerInvariant()));
            //}
            //if (filterOptions.IdPremisesType.HasValue)
            //{
            //    query = query.Where(p => p.IdPremisesTypeNavigation.IdPremisesType == filterOptions.IdPremisesType.Value);
            //}
            //if (filterOptions.IdObjectState.HasValue)
            //{
            //    query = query.Where(p => p.IdStateNavigation.IdState == filterOptions.IdObjectState.Value);
            //}
            //if (filterOptions.IdFundType.HasValue)
            //{
            //    var idPremises = registryContext.FundsPremisesAssoc
            //        .Include(fpa => fpa.IdPremisesNavigation)
            //        .Include(fpa => fpa.IdFundNavigation)
            //            .ThenInclude(fh => fh.IdFundTypeNavigation)
            //        .Where(fpa =>
            //            fpa.IdFundNavigation.ExcludeRestrictionDate == null &&
            //            fpa.IdFundNavigation.IdFundType == filterOptions.IdFundType.Value)
            //        .Select(fpa => fpa.IdPremises);

            //    query = query.Where(p => idPremises.Contains(p.IdPremises));
            //}
            return query;
        }

        private IQueryable<OwnerProcesses> GetQueryOrder(IQueryable<OwnerProcesses> query, OrderOptions orderOptions)
        {
            //if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdPremises")
            //{
            //    if (orderOptions.OrderDirection == OrderDirection.Ascending)
            //        return query.OrderBy(p => p.IdPremises);
            //    else
            //        return query.OrderByDescending(p => p.IdPremises);
            //}
            //if (orderOptions.OrderField == "PremisesType")
            //{
            //    if (orderOptions.OrderDirection == OrderDirection.Ascending)
            //        return query.OrderBy(p => p.IdPremisesTypeNavigation.PremisesType);
            //    else
            //        return query.OrderByDescending(p => p.IdPremisesTypeNavigation.PremisesType);
            //}
            //if (orderOptions.OrderField == "ObjectState")
            //{
            //    if (orderOptions.OrderDirection == OrderDirection.Ascending)
            //        return query.OrderBy(p => p.IdStateNavigation.StateNeutral);
            //    else
            //        return query.OrderByDescending(p => p.IdStateNavigation.StateNeutral);
            //}
            return query;
        }

        public List<OwnerProcesses> GetQueryPage(IQueryable<OwnerProcesses> query, PageOptions pageOptions)
            => query
            .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
            .Take(pageOptions.SizePage).ToList();


        public OwnerProcessEditVM GetCreateViewModel()
        {
            var viewModel = new OwnerProcessEditVM();
            viewModel.OwnerTypes = registryContext.OwnerType;

            return viewModel;
        }

        public void Create(OwnerProcessEditVM viewModel)
        {
            var ownerProcesses = viewModel.OwnerProcess;
            var entityEntry = registryContext.OwnerProcesses.Add(ownerProcesses);
            registryContext.SaveChanges();

            foreach (var addr in viewModel.Addresses)
            {
                switch (addr.IdTypeAddress)
                {
                    case 1:
                        var ownerBuildingAssoc = new OwnerBuildingsAssoc()
                        {
                            IdProcess = ownerProcesses.IdProcess,
                            IdBuilding = addr.IdBuilding
                        };
                        registryContext.OwnerBuildingsAssoc.Add(ownerBuildingAssoc);
                        break;
                    case 2:
                        var ownerPremisesAssoc = new OwnerPremisesAssoc()
                        {
                            IdProcess = ownerProcesses.IdProcess,
                            IdPremises = addr.IdPremise
                        };
                        registryContext.OwnerPremisesAssoc.Add(ownerPremisesAssoc);
                        break;
                    default:
                        var ownerSubPremisesAssoc = new OwnerSubPremisesAssoc()
                        {
                            IdProcess = ownerProcesses.IdProcess,
                            IdSubPremises = addr.IdSubPremise
                        };
                        registryContext.OwnerSubPremisesAssoc.Add(ownerSubPremisesAssoc);
                        break;
                }
                registryContext.SaveChanges();
            }

            foreach (var reas in viewModel.OwnerReasons)
            {
                reas.IdProcess = ownerProcesses.IdProcess;
                registryContext.OwnerReasons.Add(reas);
                registryContext.SaveChanges();
            }
        }
    }
}