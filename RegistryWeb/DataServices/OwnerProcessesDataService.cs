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
    public class OwnerProcessesDataService : ListDataService<OwnerProcessesVM, OwnerProcessesFilter>
    {
        private IEnumerable<OwnerProcess> ownerProcesses;

        public OwnerProcessesDataService(RegistryContext registryContext) : base(registryContext)
        {
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
            ownerProcesses = GetQuery();
            viewModel.PageOptions.TotalRows = ownerProcesses.Count();
            var query = GetQueryFilter(ownerProcesses, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.OwnerProcesses = GetQueryPage(query, viewModel.PageOptions);
            return viewModel;
        }

        private IEnumerable<OwnerProcess> GetQuery()
        {
            var ownerProcesses = registryContext.OwnerProcesses
                .Include(op => op.OwnerBuildingsAssoc)
                .Include(op => op.OwnerPremisesAssoc)
                .Include(op => op.OwnerSubPremisesAssoc);
            foreach (var op in ownerProcesses)
            {
                foreach (var oba in op.OwnerBuildingsAssoc)
                {
                    registryContext.Entry(oba).Reference(e => e.IdBuildingNavigation).Load();
                    registryContext.Entry(oba.IdBuildingNavigation).Reference(e => e.IdStreetNavigation).Load();
                }
                foreach (var opa in op.OwnerPremisesAssoc)
                {
                    registryContext.Entry(opa).Reference(e => e.IdPremisesNavigation).Load();
                    registryContext.Entry(opa.IdPremisesNavigation).Reference(e => e.IdPremisesTypeNavigation).Load();
                    registryContext.Entry(opa.IdPremisesNavigation).Reference(e => e.IdBuildingNavigation).Load();
                    registryContext.Entry(opa.IdPremisesNavigation.IdBuildingNavigation).Reference(e => e.IdStreetNavigation).Load();
                }
                foreach (var ospa in op.OwnerSubPremisesAssoc)
                {
                    registryContext.Entry(ospa).Reference(e => e.IdSubPremisesNavigation).Load();
                    registryContext.Entry(ospa.IdSubPremisesNavigation).Reference(e => e.IdPremisesNavigation).Load();
                    registryContext.Entry(ospa.IdSubPremisesNavigation.IdPremisesNavigation).Reference(e => e.IdPremisesTypeNavigation).Load();
                    registryContext.Entry(ospa.IdSubPremisesNavigation.IdPremisesNavigation).Reference(e => e.IdBuildingNavigation).Load();
                    registryContext.Entry(ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation).Reference(e => e.IdStreetNavigation).Load();
                }
            }
            return ownerProcesses.ToList();
        }

        private IEnumerable<OwnerProcess> GetQueryFilter(IEnumerable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            query = AddressFilter(query, filterOptions);
            query = OwnerTypeFilter(query, filterOptions);
            query = IdProcessFilter(query, filterOptions);
            return query;
        }

        private IEnumerable<OwnerProcess> AddressFilter(IEnumerable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            if (filterOptions.Address.AddressType == AddressTypes.None ||
                string.IsNullOrWhiteSpace(filterOptions.Address.Id) ||
                string.IsNullOrWhiteSpace(filterOptions.Address.Text))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return ownerProcesses.Where(op =>
                    op.OwnerBuildingsAssoc.Any(oba => oba.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id)) ||
                    op.OwnerPremisesAssoc.Any(opa => opa.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id)) ||
                    op.OwnerSubPremisesAssoc.Any(ospa => ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id)));
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                return ownerProcesses.Where(op =>
                    op.OwnerBuildingsAssoc.Any(oba => oba.IdBuildingNavigation.IdBuilding == id) ||
                    op.OwnerPremisesAssoc.Any(opa => opa.IdPremisesNavigation.IdBuildingNavigation.IdBuilding == id) ||
                    op.OwnerSubPremisesAssoc.Any(ospa => ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.IdBuilding == id));
            }
            if (filterOptions.Address.AddressType == AddressTypes.Premise)
            {
                return ownerProcesses.Where(op =>
                    op.OwnerPremisesAssoc.Any(opa => opa.IdPremisesNavigation.IdPremises == id) ||
                    op.OwnerSubPremisesAssoc.Any(ospa => ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdPremises == id));
            }
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                return ownerProcesses.Where(op =>
                    op.OwnerSubPremisesAssoc.Any(oba => oba.IdSubPremisesNavigation.IdSubPremises == id));
            }
            return query;
        }

        private IEnumerable<OwnerProcess> OwnerTypeFilter(IEnumerable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
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

        private IEnumerable<OwnerProcess> IdProcessFilter(IEnumerable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            if (filterOptions.IdProcess == null || filterOptions.IdProcess.Value == 0)
                return query;
            return query.Where(p => p.IdProcess == filterOptions.IdProcess.Value);
        }

        private IEnumerable<OwnerProcess> GetQueryOrder(IEnumerable<OwnerProcess> query, OrderOptions orderOptions)
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

        private IEnumerable<OwnerProcess> GetQueryPage(IEnumerable<OwnerProcess> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        internal OwnerProcess CreateOwnerProcess()
        {
            var ownerProcess = new OwnerProcess();
            ownerProcess.OwnerBuildingsAssoc = new List<OwnerBuildingAssoc>() { new OwnerBuildingAssoc() };
            ownerProcess.OwnerPremisesAssoc = new List<OwnerPremiseAssoc>();
            ownerProcess.OwnerSubPremisesAssoc = new List<OwnerSubPremiseAssoc>();
            var owner = new Owner() { IdOwnerType = 1 };
            owner.IdOwnerTypeNavigation = GetOwnerType(1);
            owner.OwnerPerson = new OwnerPerson();
            owner.OwnerReasons = new List<OwnerReason>() { new OwnerReason() };
            ownerProcess.Owners = new List<Owner>() { owner };
            return ownerProcess;
        }

        internal IEnumerable<OwnerType> GetOwnerTypes()
            => registryContext.OwnerType.AsNoTracking();

        internal OwnerType GetOwnerType(int idOwnerType)
            => registryContext.OwnerType.FirstOrDefault(ot => ot.IdOwnerType == idOwnerType);

        internal OwnerProcess GetOwnerProcess(int idProcess)
        {
            var ownerProcess = registryContext.OwnerProcesses
                .Include(op => op.OwnerBuildingsAssoc)
                .Include(op => op.OwnerPremisesAssoc)
                .Include(op => op.OwnerSubPremisesAssoc)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.IdOwnerTypeNavigation)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.OwnerPerson)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.OwnerOrginfo)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.OwnerReasons)
                .AsNoTracking()
                .FirstOrDefault(op => op.IdProcess == idProcess);
            return ownerProcess;
        }

        internal void Create(OwnerProcess ownerProcess)
        {
            registryContext.OwnerProcesses.Add(ownerProcess);
            registryContext.SaveChanges();
        }

        internal void Delete(int idProcess)
        {
            var ownerProcess = registryContext.OwnerProcesses
                .Include(op => op.OwnerBuildingsAssoc)
                .Include(op => op.OwnerPremisesAssoc)
                .Include(op => op.OwnerSubPremisesAssoc)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.OwnerReasons)
                .FirstOrDefault(op => op.IdProcess == idProcess);
            ownerProcess.Deleted = 1;
            foreach (var o in ownerProcess.OwnerBuildingsAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcess.OwnerPremisesAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcess.OwnerSubPremisesAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var owner in ownerProcess.Owners)
            {
                owner.Deleted = 1;
                foreach (var reason in owner.OwnerReasons)
                {
                    reason.Deleted = 1;
                }
            }
            registryContext.SaveChanges();
        }

        internal void Edit(OwnerProcess newOwnerProcess)
        {
            //Удаление
            var oldOwnerProcess = GetOwnerProcess(newOwnerProcess.IdProcess);
            foreach (var oldOwner in oldOwnerProcess.Owners)
            {
                if (newOwnerProcess.Owners.Select(owp => owp.IdOwner).Contains(oldOwner.IdOwner) == false)
                {
                    registryContext.Entry(oldOwner).Property(p => p.Deleted).IsModified = true;
                    oldOwner.Deleted = 1;
                    //случай, когда удаляется собственник. Все его документы должны удалиться автоматом
                    foreach (var oldReason in oldOwner.OwnerReasons)
                    {
                        registryContext.Entry(oldReason).Property(p => p.Deleted).IsModified = true;
                        oldReason.Deleted = 1;
                    }
                    newOwnerProcess.Owners.Add(oldOwner);
                }
                else
                {
                    //случай, когда удаляется документ.
                    var newOwner = newOwnerProcess.Owners.FirstOrDefault(ow => ow.IdOwner == oldOwner.IdOwner);
                    foreach (var oldReason in oldOwner.OwnerReasons)
                    {
                        if (newOwner.OwnerReasons.Select(or => or.IdReason).Contains(oldReason.IdReason) == false)
                        {
                            oldReason.Deleted = 1;
                            newOwner.OwnerReasons.Add(oldReason);
                        }
                    }
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

        internal IList<LogOwnerProcess> GetProcessLog(int idProcess)
        {
            var logs =
                registryContext.LogOwnerProcesses
                .Include(l => l.LogOwnerProcessesValue)
                .Include(l => l.IdLogObjectNavigation)
                .Include(l => l.IdLogTypeNavigation)
                .Where(l => l.IdProcess == idProcess)
                .ToList();
            return logs;
        }
    }
}