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


        public OwnerProcessVM CreateViewModel()
        {
            var viewModel = new OwnerProcessVM();
            viewModel.OwnerTypes = registryContext.OwnerType;

            viewModel.OwnerProcess = new OwnerProcesses();
            viewModel.Addresses = new List<Address>() { new Address() };
            viewModel.OwnerReasons = new List<OwnerReasons>() { new OwnerReasons() };
            viewModel.OwnerPersons = new List<OwnerPersons>() { new OwnerPersons() };

            return viewModel;
        }

        public OwnerProcessVM GetViewModel(int idProcess)
        {
            var viewModel = new OwnerProcessVM();
            viewModel.OwnerProcess = registryContext.OwnerProcesses.First(op => op.IdProcess == idProcess);
            viewModel.OwnerTypes = registryContext.OwnerType;

            var addresses = new List<Address>();
            foreach (var oba in registryContext.OwnerBuildingsAssoc.Where(o => o.IdProcess == idProcess))
            {
                var address = new Address();
                address.IdTypeAddress = 1;
                address.IdStreet = registryContext.Buildings
                    .First(b => b.IdBuilding == oba.IdBuilding).IdStreet;
                address.IdBuilding = oba.IdBuilding;
                addresses.Add(address);
            }
            foreach (var opa in registryContext.OwnerPremisesAssoc.Where(o => o.IdProcess == idProcess))
            {
                var address = new Address();
                address.IdTypeAddress = 2;
                var premise = registryContext.Premises
                    .First(p => p.IdPremises == opa.IdPremises);
                address.IdStreet = registryContext.Buildings
                    .First(b => b.IdBuilding == premise.IdBuilding).IdStreet;
                address.IdBuilding = premise.IdBuilding;
                address.IdPremisesType = premise.IdPremisesType;
                address.IdPremise = premise.IdPremises;
                addresses.Add(address);
            }
            foreach (var ospa in registryContext.OwnerSubPremisesAssoc.Where(o => o.IdProcess == idProcess))
            {
                var address = new Address();
                address.IdTypeAddress = 3;
                var subPremise = registryContext.SubPremises
                    .First(sp => sp.IdSubPremises == ospa.IdSubPremises);
                var premise = registryContext.Premises
                    .First(p => p.IdPremises == subPremise.IdPremises);
                address.IdStreet = registryContext.Buildings
                    .First(b => b.IdBuilding == premise.IdBuilding).IdStreet;
                address.IdBuilding = premise.IdBuilding;
                address.IdPremisesType = premise.IdPremisesType;
                address.IdPremise = premise.IdPremises;
                address.IdSubPremise = subPremise.IdSubPremises;
                addresses.Add(address);
            }
            viewModel.Addresses = addresses;

            viewModel.OwnerReasons = registryContext.OwnerReasons
                .Where(o => o.IdProcess == idProcess).ToList();
            viewModel.OwnerPersons = registryContext.OwnerPersons
                .Where(o => o.IdOwnerProcess == idProcess).ToList();
            viewModel.OwnerOrginfos = registryContext.OwnerOrginfos
                .Where(o => o.IdProcess == idProcess).ToList();

            return viewModel;
        }

        public void Create(OwnerProcessVM viewModel)
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

            if (ownerProcesses.IdOwnerType == 1)
                foreach (var pers in viewModel.OwnerPersons)
                {
                    pers.IdOwnerProcess = ownerProcesses.IdProcess;
                    registryContext.OwnerPersons.Add(pers);
                    registryContext.SaveChanges();
                }
            else
                foreach (var org in viewModel.OwnerOrginfos)
                {
                    org.IdProcess = ownerProcesses.IdProcess;
                    registryContext.OwnerOrginfos.Add(org);
                    registryContext.SaveChanges();
                }
        }
    }
}