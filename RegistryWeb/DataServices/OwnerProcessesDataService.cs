﻿using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace RegistryWeb.DataServices
{
    public class OwnerProcessesDataService : ListDataService<OwnerProcessesVM, OwnerProcessesFilter>
    {
        private readonly IQueryable<OwnerBuildingAssoc> ownerBuildingsAssoc;
        private readonly IQueryable<OwnerPremiseAssoc> ownerPremisesAssoc;
        private readonly IQueryable<OwnerSubPremiseAssoc> ownerSubPremisesAssoc;

        public OwnerProcessesDataService(RegistryContext registryContext) : base(registryContext)
        {
            ownerBuildingsAssoc = registryContext.OwnerBuildingsAssoc
                .Include(oba => oba.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(oba => oba.IdProcessNavigation)                                              
                .AsNoTracking();
            ownerPremisesAssoc = registryContext.OwnerPremisesAssoc
                .Include(opa => opa.IdPremisesNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.IdPremisesNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            ownerSubPremisesAssoc = registryContext.OwnerSubPremisesAssoc
                .Include(ospa => ospa.IdSubPremisesNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.IdSubPremisesNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
        }

        public override OwnerProcessesVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, OwnerProcessesFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }

        internal OwnerProcessesVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            OwnerProcessesFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.OwnerProcesses = GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.Addresses = GetAddresses(viewModel.OwnerProcesses);
            return viewModel;
        }

        internal IQueryable<OwnerProcess> GetQuery()
        {
            return registryContext.OwnerProcesses.AsNoTracking();
        }

        private Dictionary<int, IEnumerable<string>> GetAddresses(IEnumerable<OwnerProcess> ownerProcesses)
        {
            var addresses = new Dictionary<int, IEnumerable<string>>();
            var buildingsAssoc = ownerBuildingsAssoc.ToList();
            var premisesAssoc = ownerPremisesAssoc.ToList();
            var subPremisesAssoc = ownerSubPremisesAssoc.ToList();
            foreach (var ownerProcess in ownerProcesses)
            {
                var curOwnerProcessAddresses = new List<string>();
                foreach (var oba in buildingsAssoc.Where(oba => oba.IdProcess == ownerProcess.IdProcess))
                {
                    curOwnerProcessAddresses.Add(
                        oba.IdBuildingNavigation.IdStreetNavigation.StreetName +
                        ", д." + oba.IdBuildingNavigation.House);
                }
                foreach (var opa in premisesAssoc.Where(opa => opa.IdProcess == ownerProcess.IdProcess))
                {
                    curOwnerProcessAddresses.Add(
                        opa.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName +
                        ", д." + opa.IdPremisesNavigation.IdBuildingNavigation.House + ", " +
                        opa.IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                        opa.IdPremisesNavigation.PremisesNum);
                }
                foreach (var ospa in subPremisesAssoc.Where(ospa => ospa.IdProcess == ownerProcess.IdProcess))
                {
                    curOwnerProcessAddresses.Add(
                        ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName
                        + ", д." + ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.House + ", " +
                        ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                        ospa.IdSubPremisesNavigation.IdPremisesNavigation.PremisesNum +
                        ", к." + ospa.IdSubPremisesNavigation.SubPremisesNum);
                }
                addresses.Add(ownerProcess.IdProcess, curOwnerProcessAddresses);
            }
            return addresses;
        }

        private IQueryable<OwnerProcess> GetQueryFilter(IQueryable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        { 
            query = AddressFilter(query, filterOptions);
            query = OwnerTypeFilter(query, filterOptions);
            query = IdProcessFilter(query, filterOptions);
            return query;
        }

        private IQueryable<OwnerProcess> AddressFilter(IQueryable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            if (filterOptions.Address.AddressType == AddressTypes.None ||
                string.IsNullOrWhiteSpace(filterOptions.Address.Id) ||
                string.IsNullOrWhiteSpace(filterOptions.Address.Text))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                var idBuildingProcesses = ownerBuildingsAssoc
                    .Where(oba => oba.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = ownerPremisesAssoc
                    .Where(opa => opa.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = ownerSubPremisesAssoc
                    .Where(ospa => ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                //query.Join(idProcesses, q => q.IdProcess, idProc => idProc, (q, idProc) => q);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                var idBuildingProcesses = ownerBuildingsAssoc
                    .Where(oba => oba.IdBuilding == id)
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = ownerPremisesAssoc
                    .Where(opa => opa.IdPremisesNavigation.IdBuilding == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = ownerSubPremisesAssoc
                    .Where(ospa => ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuilding == id)
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            if (filterOptions.Address.AddressType == AddressTypes.Premise)
            {
                var idPremiseProcesses = ownerPremisesAssoc
                    .Where(opa => opa.IdPremisesNavigation.IdPremises == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = ownerSubPremisesAssoc
                    .Where(ospa => ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdPremises == id)
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idPremiseProcesses.Union(idSubPremiseProcesses);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                var idProcesses = ownerSubPremisesAssoc
                    .Where(ospa => ospa.IdSubPremisesNavigation.IdSubPremises == id)
                    .Select(ospa => ospa.IdProcess);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            return query;
        }

        private IQueryable<OwnerProcess> OwnerTypeFilter(IQueryable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            if (filterOptions.IdOwnerType == null || filterOptions.IdOwnerType.Value == 0)
                return query;
            var result =
                from process in query
                join owner in registryContext.Owners
                    on process.IdProcess equals owner.IdProcess
                where owner.IdOwnerType == filterOptions.IdOwnerType.Value
                select process;
            return result.Distinct();
        }

        private IQueryable<OwnerProcess> IdProcessFilter(IQueryable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            if (filterOptions.IdProcess == null || filterOptions.IdProcess.Value == 0)
                return query;
            return query.Where(p => p.IdProcess == filterOptions.IdProcess.Value);
        }

        private IQueryable<OwnerProcess> GetQueryOrder(IQueryable<OwnerProcess> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdProcess")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdProcess);
                else
                    return query.OrderByDescending(p => p.IdProcess);
            }
            return query;
        }

        internal IQueryable<OwnerProcess> GetQueryPage(IQueryable<OwnerProcess> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        internal OwnerProcess CreateOwnerProcess()
        {
            var owner = new Owner();
            owner.OwnerReasons = new List<OwnerReason>() { new OwnerReason() };
            var ownerProcess = new OwnerProcess();
            ownerProcess.OwnerBuildingsAssoc = new List<OwnerBuildingAssoc>() { new OwnerBuildingAssoc() };
            ownerProcess.OwnerPremisesAssoc = new List<OwnerPremiseAssoc>();
            ownerProcess.OwnerSubPremisesAssoc = new List<OwnerSubPremiseAssoc>();
            ownerProcess.Owners = new List<Owner>() { owner };
            return ownerProcess;
        }

        internal IEnumerable<OwnerType> GetOwnerTypes
            => registryContext.OwnerType.AsNoTracking();

        internal OwnerProcess GetOwnerProcess(int idProcess)
        {
            return registryContext.OwnerProcesses
                .Include(op => op.OwnerBuildingsAssoc)
                .Include(op => op.OwnerPremisesAssoc)
                .Include(op => op.OwnerSubPremisesAssoc)
                .Include(op => op.Owners)
                .AsNoTracking()
                .First(op => op.IdProcess == idProcess);
        }

        internal void Create(OwnerProcess ownerProcess)
        {
            registryContext.OwnerProcesses.Add(ownerProcess);
            registryContext.SaveChanges();
        }

        internal void Delete(int idProcess)
        {
            var ownerProcesses = registryContext.OwnerProcesses
                .Include(op => op.OwnerBuildingsAssoc)
                .Include(op => op.OwnerPremisesAssoc)
                .Include(op => op.OwnerSubPremisesAssoc)
                .Include(op => op.Owners)
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
            //foreach (var o in ownerProcesses.OwnerReasons)
            //{
            //    o.Deleted = 1;
            //}
            foreach (var o in ownerProcesses.Owners)
            {
                o.Deleted = 1;
            }
        }

        internal void Edit(OwnerProcess newOwnerProcess)
        {
            //Удаление
            var oldOwnerProcess = GetOwnerProcess(newOwnerProcess.IdProcess);
            //foreach (var or in oldOwnerProcess.OwnerReasons)
            //{
            //    if (newOwnerProcess.OwnerReasons.Select(owr => owr.IdReason).Contains(or.IdReason) == false)
            //    {
            //        or.Deleted = 1;
            //        newOwnerProcess.OwnerReasons.Add(or);
            //    }
            //}
            foreach (var owner in oldOwnerProcess.Owners)
            {
                if (newOwnerProcess.Owners.Select(owp => owp.IdOwner).Contains(owner.IdOwner) == false)
                {
                    owner.Deleted = 1;
                    newOwnerProcess.Owners.Add(owner);
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

        internal void Annul(OwnerProcess newOwnerProcess)
        {
            registryContext.OwnerProcesses.Update(newOwnerProcess);
            registryContext.SaveChanges();
        }
    }
}