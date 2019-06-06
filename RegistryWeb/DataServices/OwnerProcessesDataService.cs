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
    public class OwnerProcessesDataService : ListDataService<OwnerProcessesListVM, OwnerProcessesListFilter>
    {
        public OwnerProcessesDataService(RegistryContext registryContext) : base(registryContext)
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
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage).ToList();
        }

        public OwnerProcesses CreateOwnerProcess()
        {
            var ownerProcess = new OwnerProcesses() { IdOwnerType = 1 };
            ownerProcess.OwnerBuildingsAssoc = new List<OwnerBuildingsAssoc>() { new OwnerBuildingsAssoc() };
            ownerProcess.OwnerPremisesAssoc = new List<OwnerPremisesAssoc>();
            ownerProcess.OwnerSubPremisesAssoc = new List<OwnerSubPremisesAssoc>();
            ownerProcess.OwnerReasons = new List<OwnerReasons>() { new OwnerReasons() };
            ownerProcess.OwnerPersons = new List<OwnerPersons>() { new OwnerPersons() };
            ownerProcess.OwnerOrginfos = new List<OwnerOrginfos>() { new OwnerOrginfos() };
            return ownerProcess;
        }

        public IEnumerable<OwnerType> GetOwnerTypes
            => registryContext.OwnerType.AsNoTracking();

        public OwnerProcesses GetOwnerProcess(int idProcess)
        {
            return registryContext.OwnerProcesses
                .Include(op => op.OwnerBuildingsAssoc)
                .Include(op => op.OwnerPremisesAssoc)
                .Include(op => op.OwnerSubPremisesAssoc)
                .Include(op => op.OwnerReasons)
                .Include(op => op.OwnerPersons)
                .Include(op => op.OwnerOrginfos)
                .AsNoTracking()
                .First(op => op.IdProcess == idProcess);
        }

        public void Create(OwnerProcesses ownerProcess)
        {
            registryContext.OwnerProcesses.Add(ownerProcess);
            registryContext.SaveChanges();
        }

        public void Delete(int idProcess)
        {
            var ownerProcesses = registryContext.OwnerProcesses
                .Include(op => op.OwnerBuildingsAssoc)
                .Include(op => op.OwnerPremisesAssoc)
                .Include(op => op.OwnerSubPremisesAssoc)
                .Include(op => op.OwnerReasons)
                .Include(op => op.OwnerPersons)
                .Include(op => op.OwnerOrginfos)
                .First(op => op.IdProcess == idProcess);
            ownerProcesses.Deleted = 1;
            foreach(var o in ownerProcesses.OwnerBuildingsAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcesses.OwnerPremisesAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcesses.OwnerSubPremisesAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcesses.OwnerReasons)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcesses.OwnerPersons)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcesses.OwnerOrginfos)
            {
                o.Deleted = 1;
            }
            registryContext.SaveChanges();
        }

        public void Edit(OwnerProcesses newOwnerProcess)
        {
            //Удаление
            var oldOwnerProcess = GetOwnerProcess(newOwnerProcess.IdProcess);
            foreach (var or in oldOwnerProcess.OwnerReasons)
            {
                if (newOwnerProcess.OwnerReasons.Select(owr => owr.IdReason).Contains(or.IdReason) == false)
                {
                    or.Deleted = 1;
                    newOwnerProcess.OwnerReasons.Add(or);
                }
            }
            foreach (var op in oldOwnerProcess.OwnerPersons)
            {
                if (newOwnerProcess.OwnerPersons.Select(owp => owp.IdOwnerPersons).Contains(op.IdOwnerPersons) == false)
                {
                    op.Deleted = 1;
                    newOwnerProcess.OwnerPersons.Add(op);
                }
            }
            foreach (var oba in oldOwnerProcess.OwnerBuildingsAssoc)
            {
                if (newOwnerProcess.OwnerBuildingsAssoc.Select(owba => owba.IdAssoc).Contains(oba.IdAssoc) == false)
                {
                    oba.Deleted = 1;
                    newOwnerProcess.OwnerBuildingsAssoc.Add(oba);
                }
            }
            foreach (var opa in oldOwnerProcess.OwnerPremisesAssoc)
            {
                if (newOwnerProcess.OwnerPremisesAssoc.Select(owpa => owpa.IdAssoc).Contains(opa.IdAssoc) == false)
                {
                    opa.Deleted = 1;
                    newOwnerProcess.OwnerPremisesAssoc.Add(opa);
                }
            }
            foreach (var ospa in oldOwnerProcess.OwnerSubPremisesAssoc)
            {
                if (newOwnerProcess.OwnerSubPremisesAssoc.Select(owspa => owspa.IdAssoc).Contains(ospa.IdAssoc) == false)
                {
                    ospa.Deleted = 1;
                    newOwnerProcess.OwnerSubPremisesAssoc.Add(ospa);
                }
            }
            //Добавление и радактирование
            registryContext.OwnerProcesses.Update(newOwnerProcess);
            registryContext.SaveChanges();
        }
    }
}