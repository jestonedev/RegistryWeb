using RegistryDb.Models;
using System.Linq;
using RegistryWeb.ViewOptions;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities.Owners;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;
using RegistryWeb.Enums;
using RegistryWeb.DataServices;

namespace RegistryServices.DataFilterServices
{
    class OwnerProcessesFilterService : AbstractFilterService<OwnerProcess, OwnerProcessesFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly AddressesDataService addressesDataService;
        private readonly IQueryable<OwnerBuildingAssoc> ownerBuildingsAssoc;
        private readonly IQueryable<OwnerPremiseAssoc> ownerPremisesAssoc;
        private readonly IQueryable<OwnerSubPremiseAssoc> ownerSubPremisesAssoc;

        public OwnerProcessesFilterService(RegistryContext registryContext, AddressesDataService addressesDataService)
        {
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
            ownerBuildingsAssoc = registryContext.OwnerBuildingsAssoc
                .Include(oba => oba.BuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            ownerPremisesAssoc = registryContext.OwnerPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            ownerSubPremisesAssoc = registryContext.OwnerSubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
        }

        public override IQueryable<OwnerProcess> GetQueryFilter(IQueryable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = OwnerTypeFilter(query, filterOptions);
                query = IdProcessFilter(query, filterOptions);
                query = IdProcessTypeFilter(query, filterOptions);
                query = GetModalAddressFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<OwnerProcess> GetModalAddressFilter(IQueryable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            if (filterOptions.IdStreet == null && filterOptions.House == null && filterOptions.PremisesNum == null)
                return query;
            IEnumerable<int> idsProcess = null;
            var buildingsAssoc = ownerBuildingsAssoc.ToList();
            var premisesAssoc = ownerPremisesAssoc.ToList();
            var subPremisesAssoc = ownerSubPremisesAssoc.ToList();
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var ids = buildingsAssoc
                    .Where(r => r.BuildingNavigation.IdStreet == filterOptions.IdStreet)
                    .Select(r => r.IdProcess)
                    .Union(premisesAssoc
                        .Where(r => r.PremiseNavigation.IdBuildingNavigation.IdStreet == filterOptions.IdStreet)
                        .Select(r => r.IdProcess))
                    .Union(subPremisesAssoc
                        .Where(r => r.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet == filterOptions.IdStreet)
                        .Select(r => r.IdProcess));
                idsProcess = ids;
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var ids = buildingsAssoc
                    .Where(r => r.BuildingNavigation.House == filterOptions.House)
                    .Select(r => r.IdProcess)
                    .Union(premisesAssoc
                        .Where(r => r.PremiseNavigation.IdBuildingNavigation.House == filterOptions.House)
                        .Select(r => r.IdProcess))
                    .Union(subPremisesAssoc
                        .Where(r => r.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House == filterOptions.House)
                        .Select(r => r.IdProcess));
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var ids = premisesAssoc
                    .Where(r => r.PremiseNavigation.PremisesNum == filterOptions.PremisesNum)
                    .Select(r => r.IdProcess)
                    .Union(subPremisesAssoc
                        .Where(r => r.SubPremiseNavigation.IdPremisesNavigation.PremisesNum == filterOptions.PremisesNum)
                        .Select(r => r.IdProcess));
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (idsProcess != null)
            {
                query = (from row in query
                         join id in idsProcess
                         on row.IdProcess equals id
                         select row).Distinct();
            }
            return query;
        }

        private IQueryable<OwnerProcess> AddressFilter(IQueryable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);
            if (filterOptions.Address.Id != null)
            {
                addresses = new List<string> { filterOptions.Address.Id };
            }

            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                var idBuildingProcesses = ownerBuildingsAssoc
                    .Where(oba => addresses.Contains(oba.BuildingNavigation.IdStreet))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = ownerPremisesAssoc
                    .Where(opa => addresses.Contains(opa.PremiseNavigation.IdBuildingNavigation.IdStreet))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = ownerSubPremisesAssoc
                    .Where(ospa => addresses.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet))
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                //query.Join(idProcesses, q => q.IdProcess, idProc => idProc, (q, idProc) => q);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));
            if (!addressesInt.Any())
                return query;

            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                var idBuildingProcesses = ownerBuildingsAssoc
                    .Where(oba => addressesInt.Contains(oba.IdBuilding))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = ownerPremisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdBuilding))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = ownerSubPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding))
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
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdPremises))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = ownerSubPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises))
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
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdSubPremises))
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

        private IQueryable<OwnerProcess> IdProcessTypeFilter(IQueryable<OwnerProcess> query, OwnerProcessesFilter filterOptions)
        {
            //Все
            if (filterOptions.IdProcessType == null || filterOptions.IdProcessType.Value == 0)
                return query;
            // Действующие
            if (filterOptions.IdProcessType == 1)
                return query.Where(p => p.AnnulDate == null);
            // 2 Аннулированные
            return query.Where(p => p.AnnulDate != null);
        }

        public override IQueryable<OwnerProcess> GetQueryIncludes(IQueryable<OwnerProcess> query)
        {
            return query;
        }

        public override IQueryable<OwnerProcess> GetQueryOrder(IQueryable<OwnerProcess> query, OrderOptions orderOptions)
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
    }
}
