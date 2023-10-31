using RegistryDb.Models;
using RegistryWeb.DataServices;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Enums;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryServices.DataFilterServices
{
    class TenancyProcessFilterService : AbstractFilterService<TenancyProcess, TenancyProcessesFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly AddressesDataService addressesDataService;
        private readonly IQueryable<TenancyBuildingAssoc> tenancyBuildingsAssoc;
        private readonly IQueryable<TenancyPremiseAssoc> tenancyPremisesAssoc;
        private readonly IQueryable<TenancySubPremiseAssoc> tenancySubPremisesAssoc;

        public TenancyProcessFilterService(RegistryContext registryContext, AddressesDataService addressesDataService)
        {
            tenancyBuildingsAssoc = registryContext.TenancyBuildingsAssoc
                       .Include(oba => oba.BuildingNavigation)
                       .ThenInclude(b => b.IdStreetNavigation)
                       .Include(oba => oba.ProcessNavigation)
                   .AsNoTracking();
            tenancyPremisesAssoc = registryContext.TenancyPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            tenancySubPremisesAssoc = registryContext.TenancySubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
        }

        public override IQueryable<TenancyProcess> GetQueryFilter(IQueryable<TenancyProcess> query, TenancyProcessesFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = TenancyFilter(query, filterOptions);
                query = MunObjectFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<TenancyProcess> AddressFilter(IQueryable<TenancyProcess> query, TenancyProcessesFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                var idBuildingProcesses = tenancyBuildingsAssoc
                    .Where(oba => addresses.Contains(oba.BuildingNavigation.IdStreet))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = tenancyPremisesAssoc
                    .Where(opa => addresses.Contains(opa.PremiseNavigation.IdBuildingNavigation.IdStreet))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
                    .Where(ospa => addresses.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet))
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
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
                var idBuildingProcesses = tenancyBuildingsAssoc
                    .Where(oba => addressesInt.Contains(oba.IdBuilding))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = tenancyPremisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdBuilding))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
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
                var idPremiseProcesses = tenancyPremisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdPremises))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
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
                var idProcesses = tenancySubPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdSubPremises))
                    .Select(ospa => ospa.IdProcess);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            return query;
        }

        private IQueryable<TenancyProcess> TenancyFilter(IQueryable<TenancyProcess> query, TenancyProcessesFilter filterOptions)
        {
            if (filterOptions.IdProcess != null && filterOptions.IdProcess != 0)
            {
                query = query.Where(p => p.IdProcess == filterOptions.IdProcess.Value);
            }
            if (!string.IsNullOrEmpty(filterOptions.RegistrationNum))
            {
                query = query.Where(p => p.RegistrationNum.Contains(filterOptions.RegistrationNum));
            }
            if (filterOptions.RegistrationNumIsEmpty)
            {
                query = query.Where(p => p.RegistrationNum == null);
            }
            if (filterOptions.RegistrationDate.HasValue)
            {
                if (filterOptions.RegistrationDateSign == "=")
                    query = query.Where(p => p.RegistrationDate == filterOptions.RegistrationDate);
                if (filterOptions.RegistrationDateSign == "≥")
                    query = query.Where(p => p.RegistrationDate >= filterOptions.RegistrationDate);
                if (filterOptions.RegistrationDateSign == "≤")
                    query = query.Where(p => p.RegistrationDate <= filterOptions.RegistrationDate);
            }
            if (filterOptions.IssuedDate.HasValue)
            {
                if (filterOptions.IssuedDateSign == "=")
                    query = query.Where(p => p.IssueDate == filterOptions.IssuedDate);
                if (filterOptions.IssuedDateSign == "≥")
                    query = query.Where(p => p.IssueDate >= filterOptions.IssuedDate);
                if (filterOptions.IssuedDateSign == "≤")
                    query = query.Where(p => p.IssueDate <= filterOptions.IssuedDate);
            }
            if (filterOptions.BeginDate.HasValue)
            {
                if (filterOptions.BeginDateSign == "=")
                    query = query.Where(p => p.BeginDate == filterOptions.BeginDate);
                if (filterOptions.BeginDateSign == "≥")
                    query = query.Where(p => p.BeginDate >= filterOptions.BeginDate);
                if (filterOptions.BeginDateSign == "≤")
                    query = query.Where(p => p.BeginDate <= filterOptions.BeginDate);
            }
            if (filterOptions.EndDate.HasValue)
            {
                if (filterOptions.EndDateSign == "=")
                    query = query.Where(p => p.EndDate == filterOptions.EndDate);
                if (filterOptions.EndDateSign == "≥")
                    query = query.Where(p => p.EndDate >= filterOptions.EndDate);
                if (filterOptions.EndDateSign == "≤")
                    query = query.Where(p => p.EndDate <= filterOptions.EndDate);
            }
            if (filterOptions.IdsRentType != null && filterOptions.IdsRentType.Any())
            {
                query = query.Where(p => p.IdRentType != null && filterOptions.IdsRentType.Contains(p.IdRentType.Value));
            }
            if (!string.IsNullOrEmpty(filterOptions.ReasonDocNum))
            {
                query = query.Where(p => p.TenancyReasons.Any(tr => tr.ReasonNumber.Contains(filterOptions.ReasonDocNum)));
            }
            if (filterOptions.ReasonDocDate.HasValue)
            {
                query = query.Where(p => p.TenancyReasons.Any(tr => tr.ReasonDate == filterOptions.ReasonDocDate));
            }
            if (filterOptions.IdsReasonType != null && filterOptions.IdsReasonType.Any())
            {
                query = query.Where(p => p.TenancyReasons.Any(tr => filterOptions.IdsReasonType.Contains(tr.IdReasonType)));
            }
            if (!string.IsNullOrEmpty(filterOptions.TenantSnp) || !string.IsNullOrEmpty(filterOptions.TenancyParticipantSnp))
            {
                var tenantSnp = string.IsNullOrEmpty(filterOptions.TenantSnp) ? null : filterOptions.TenantSnp.ToLowerInvariant();
                var tenancyParticipantSnp = string.IsNullOrEmpty(filterOptions.TenancyParticipantSnp) ? null : filterOptions.TenancyParticipantSnp.ToLowerInvariant();
                query = (from tRow in query
                         join tpRow in registryContext.TenancyPersons
                         on tRow.IdProcess equals tpRow.IdProcess
                         where tpRow.ExcludeDate == null &&
                             ((tenantSnp != null && tpRow.IdKinship == 1 &&
                                 string.Concat(tpRow.Surname.Trim(), " ", tpRow.Name.Trim(), " ", tpRow.Patronymic == null ? "" : tpRow.Patronymic.Trim()).ToLowerInvariant().Contains(tenantSnp)) ||
                             (tenancyParticipantSnp != null &&
                                 string.Concat(tpRow.Surname.Trim(), " ", tpRow.Name.Trim(), " ", tpRow.Patronymic == null ? "" : tpRow.Patronymic.Trim()).ToLowerInvariant().Contains(tenancyParticipantSnp)))
                         select tRow).Distinct();
            }
            if (filterOptions.TenantBirthDate.HasValue)
            {
                query = query.Where(p => p.TenancyPersons.Any(tr => tr.DateOfBirth == filterOptions.TenantBirthDate && tr.IdKinship == 1));
            }
            if (filterOptions.TenancyParticipantBirthDate.HasValue)
            {
                query = query.Where(p => p.TenancyPersons.Any(tr => tr.DateOfBirth == filterOptions.TenancyParticipantBirthDate && tr.IdKinship != 1));
            }
            if (filterOptions.IdPreset != null)
            {
                switch (filterOptions.IdPreset)
                {
                    case 1:
                        var filterEndDate = DateTime.Now.AddMonths(4).Date;
                        var filterStartDate = DateTime.Now.Date;
                        query = from tRow in query
                                where tRow.EndDate >= filterStartDate && tRow.EndDate < filterEndDate
                                select tRow;
                        break;
                    case 2:
                        filterEndDate = DateTime.Now.Date;
                        query = from tRow in query
                                where tRow.EndDate < filterEndDate
                                select tRow;
                        break;
                    case 3:
                        query = (from tRow in query
                                 join rpRow in registryContext.TenancyRentPeriods
                                 on tRow.IdProcess equals rpRow.IdProcess into gs
                                 from gsRow in gs.DefaultIfEmpty()
                                 where gsRow != null
                                 select tRow).Distinct();
                        break;
                    case 4:
                        // В MunObjectFilter
                        break;
                    case 5:
                        filterEndDate = DateTime.Now.Date;
                        query = from tRow in query
                                where tRow.EndDate < filterEndDate && (!tRow.RegistrationNum.Contains('н'))
                                select tRow;
                        break;
                }
            }

            return query;
        }

        private IQueryable<TenancyProcess> MunObjectFilter(IQueryable<TenancyProcess> query, TenancyProcessesFilter filterOptions)
        {
            var buildings = from tbaRow in tenancyBuildingsAssoc
                            join buildingRow in registryContext.Buildings
                            on tbaRow.IdBuilding equals buildingRow.IdBuilding
                            join streetRow in registryContext.KladrStreets
                            on buildingRow.IdStreet equals streetRow.IdStreet
                            select new
                            {
                                tbaRow.IdProcess,
                                tbaRow.IdBuilding,
                                streetRow.IdStreet,
                                buildingRow.House,
                                buildingRow.IdState
                            };
            var premises = from tpaRow in tenancyPremisesAssoc
                           join premiseRow in registryContext.Premises
                           on tpaRow.IdPremise equals premiseRow.IdPremises
                           join buildingRow in registryContext.Buildings
                           on premiseRow.IdBuilding equals buildingRow.IdBuilding
                           join streetRow in registryContext.KladrStreets
                           on buildingRow.IdStreet equals streetRow.IdStreet
                           join premiseTypesRow in registryContext.PremisesTypes
                           on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                           select new
                           {
                               tpaRow.IdProcess,
                               tpaRow.IdPremise,
                               streetRow.IdStreet,
                               buildingRow.IdBuilding,
                               buildingRow.House,
                               premiseRow.PremisesNum,
                               premiseRow.IdState
                           };
            var subPremises = from tspaRow in tenancySubPremisesAssoc
                              join subPremiseRow in registryContext.SubPremises
                              on tspaRow.IdSubPremise equals subPremiseRow.IdSubPremises
                              join premiseRow in registryContext.Premises
                              on subPremiseRow.IdPremises equals premiseRow.IdPremises
                              join buildingRow in registryContext.Buildings
                              on premiseRow.IdBuilding equals buildingRow.IdBuilding
                              join streetRow in registryContext.KladrStreets
                              on buildingRow.IdStreet equals streetRow.IdStreet
                              join premiseTypesRow in registryContext.PremisesTypes
                              on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                              select new
                              {
                                  tspaRow.IdProcess,
                                  tspaRow.IdSubPremise,
                                  streetRow.IdStreet,
                                  buildingRow.IdBuilding,
                                  buildingRow.House,
                                  premiseRow.IdPremises,
                                  premiseRow.PremisesNum,
                                  subPremiseRow.SubPremisesNum,
                                  subPremiseRow.IdState
                              };
            IEnumerable<int> idsProcess = null;

            if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                var ids = buildings.Where(r => r.IdStreet.Contains(filterOptions.IdRegion)).Select(r => r.IdProcess)
                    .Union(premises.Where(r => r.IdStreet.Contains(filterOptions.IdRegion)).Select(r => r.IdProcess))
                    .Union(subPremises.Where(r => r.IdStreet.Contains(filterOptions.IdRegion)).Select(r => r.IdProcess))
                    .ToList();
                idsProcess = ids;
            }
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var ids = buildings.Where(r => r.IdStreet == filterOptions.IdStreet).Select(r => r.IdProcess)
                    .Union(premises.Where(r => r.IdStreet == filterOptions.IdStreet).Select(r => r.IdProcess))
                    .Union(subPremises.Where(r => r.IdStreet == filterOptions.IdStreet).Select(r => r.IdProcess))
                    .ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var ids = buildings.Where(r => r.House == filterOptions.House).Select(r => r.IdProcess)
                    .Union(premises.Where(r => r.House == filterOptions.House).Select(r => r.IdProcess))
                    .Union(subPremises.Where(r => r.House == filterOptions.House).Select(r => r.IdProcess))
                    .ToList();
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
                var ids = premises.Where(r => r.PremisesNum == filterOptions.PremisesNum).Select(r => r.IdProcess)
                    .Union(subPremises.Where(r => r.PremisesNum == filterOptions.PremisesNum).Select(r => r.IdProcess))
                    .ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.SubPremisesNum))
            {
                var ids = subPremises.Where(r => r.SubPremisesNum == filterOptions.SubPremisesNum).Select(r => r.IdProcess).ToList();
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
            if (filterOptions.IdsObjectState != null && filterOptions.IdsObjectState.Any())
            {
                idsProcess = buildings.Where(r => filterOptions.IdsObjectState.Any(s => s == r.IdState)).Select(r => r.IdProcess)
                    .Union(premises.Where(r => filterOptions.IdsObjectState.Any(s => s == r.IdState)).Select(r => r.IdProcess))
                    .Union(subPremises.Where(r => filterOptions.IdsObjectState.Any(s => s == r.IdState)).Select(r => r.IdProcess))
                    .ToList();
                query = (from row in query
                         join id in idsProcess
                         on row.IdProcess equals id
                         select row).Distinct();
            }

            if (filterOptions.IdSubPremises != null)
            {
                query = (from row in query
                         join subPremise in tenancySubPremisesAssoc
                         on row.IdProcess equals subPremise.IdProcess
                         where subPremise.IdSubPremise == filterOptions.IdSubPremises
                         select row).Distinct();
            }

            if (filterOptions.IdPremises != null)
            {
                var ids = premises.Where(p => p.IdPremise == filterOptions.IdPremises).Select(p => p.IdProcess).Union(
                        subPremises.Where(p => p.IdPremises == filterOptions.IdPremises).Select(p => p.IdProcess))
                        .ToList();
                query = (from row in query
                         join id in ids
                         on row.IdProcess equals id
                         select row).Distinct();
            }

            if (filterOptions.IdBuilding != null)
            {
                var ids = premises.Where(p => p.IdBuilding == filterOptions.IdBuilding).Select(p => p.IdProcess).Union(
                        subPremises.Where(p => p.IdBuilding == filterOptions.IdBuilding).Select(p => p.IdProcess)).Union(
                        buildings.Where(p => p.IdBuilding == filterOptions.IdBuilding).Select(p => p.IdProcess))
                        .ToList();
                query = (from row in query
                         join id in ids
                         on row.IdProcess equals id
                         select row).Distinct();
            }

            if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any())
            {
                var specialOwnershipRightTypeIds = new int[] { 1, 2, 6, 7, 8 };
                var specialIds = filterOptions.IdsOwnershipRightType.Where(id => specialOwnershipRightTypeIds.Contains(id));
                var generalIds = filterOptions.IdsOwnershipRightType.Where(id => !specialOwnershipRightTypeIds.Contains(id));
                var generalOwnershipRightsPremises = from owrRow in registryContext.OwnershipRights
                                                     join pRow in registryContext.OwnershipPremisesAssoc
                                                     on owrRow.IdOwnershipRight equals pRow.IdOwnershipRight
                                                     where generalIds.Contains(owrRow.IdOwnershipRightType)
                                                     select pRow.IdPremises;

                var specialOwnershipRightsPremises = from owrRow in registryContext.PremisesOwnershipRightCurrent
                                                     where specialIds.Contains(owrRow.IdOwnershipRightType)
                                                     select owrRow.IdPremises;

                var ownershipRightsPremisesList = generalOwnershipRightsPremises.Union(specialOwnershipRightsPremises).ToList();

                var generalOwnershipRightsBuildings = from owrRow in registryContext.OwnershipRights
                                                      join bRow in registryContext.OwnershipBuildingsAssoc
                                                      on owrRow.IdOwnershipRight equals bRow.IdOwnershipRight
                                                      where generalIds.Contains(owrRow.IdOwnershipRightType)
                                                      select bRow.IdBuilding;

                var specialOwnershipRightsBuildings = from owrRow in registryContext.BuildingsOwnershipRightCurrent
                                                      where specialIds.Contains(owrRow.IdOwnershipRightType)
                                                      select owrRow.IdBuilding;

                var ownershipRightsBuildingsList = generalOwnershipRightsBuildings.Union(specialOwnershipRightsBuildings).ToList(); //

                var premisesInBuildingsOwnershipRights = (from pRow in registryContext.Premises
                                                          where ownershipRightsBuildingsList.Contains(pRow.IdBuilding)
                                                          select pRow.IdPremises).ToList();

                ownershipRightsPremisesList = ownershipRightsPremisesList.Union(premisesInBuildingsOwnershipRights).ToList(); //

                var ownershipRightsSubPremisesList = (from spRow in registryContext.SubPremises
                                                      where ownershipRightsPremisesList.Contains(spRow.IdPremises)
                                                      select spRow.IdSubPremises).ToList(); //

                var buildingProcesses = from bRow in buildings
                                        where ownershipRightsBuildingsList.Contains(bRow.IdBuilding)
                                        select bRow.IdProcess;

                var premisesProcesses = from pRow in premises
                                        where ownershipRightsPremisesList.Contains(pRow.IdPremise)
                                        select pRow.IdProcess;

                var subPremisesProcesses = from spRow in subPremises
                                           where ownershipRightsSubPremisesList.Contains(spRow.IdSubPremise)
                                           select spRow.IdProcess;

                query = (from row in query
                         join id in buildingProcesses.Union(premisesProcesses).Union(subPremisesProcesses)
                         on row.IdProcess equals id
                         select row).Distinct();
            }
            if (filterOptions.IdPreset != null)
            {
                switch (filterOptions.IdPreset)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 5:
                        //В TenancyFilter
                        break;
                    case 4:
                        var ownershipRightTypeIds = new int[] { 2, 7 };
                        var ownershipRightsPremises = (from owrRow in registryContext.PremisesOwnershipRightCurrent
                                                       where ownershipRightTypeIds.Contains(owrRow.IdOwnershipRightType)
                                                       select owrRow.IdPremises).ToList();
                        var ownershipRightsBuildings = (from owrRow in registryContext.BuildingsOwnershipRightCurrent
                                                        where ownershipRightTypeIds.Contains(owrRow.IdOwnershipRightType)
                                                        select owrRow.IdBuilding).ToList();

                        var premisesInBuildingsOwnershipRights = (from pRow in registryContext.Premises
                                                                  where ownershipRightsPremises.Contains(pRow.IdBuilding)
                                                                  select pRow.IdPremises).ToList();
                        ownershipRightsPremises = ownershipRightsPremises.Union(premisesInBuildingsOwnershipRights).ToList();

                        var ownershipRightsSubPremises = (from spRow in registryContext.SubPremises
                                                          where ownershipRightsPremises.Contains(spRow.IdPremises)
                                                          select spRow.IdSubPremises).ToList();

                        var subPremisesTenancyIds = from tspaRow in registryContext.TenancySubPremisesAssoc
                                                    join sRow in registryContext.SubPremises
                                                    on tspaRow.IdSubPremise equals sRow.IdSubPremises
                                                    join id in ownershipRightsSubPremises
                                                    on sRow.IdSubPremises equals id
                                                    where sRow.IdState == 4
                                                    select tspaRow.IdProcess;

                        var premisesTenancyIds = from tpaRow in registryContext.TenancyPremisesAssoc
                                                 join pRow in registryContext.Premises
                                                 on tpaRow.IdPremise equals pRow.IdPremises
                                                 join id in ownershipRightsPremises
                                                 on pRow.IdPremises equals id
                                                 where pRow.IdState == 4
                                                 select tpaRow.IdProcess;

                        var buildingsTenancyIds = from tbaRow in registryContext.TenancyBuildingsAssoc
                                                  join bRow in registryContext.Buildings
                                                  on tbaRow.IdBuilding equals bRow.IdBuilding
                                                  join id in ownershipRightsBuildings
                                                 on bRow.IdBuilding equals id
                                                  where bRow.IdState == 4
                                                  select tbaRow.IdProcess;

                        var tenancyIds = subPremisesTenancyIds.Union(premisesTenancyIds).Union(buildingsTenancyIds).ToList();

                        query = from row in query
                                where row.IdRentType == 1 && tenancyIds.Contains(row.IdProcess)
                                select row;

                        break;
                }
            }
            return query;
        }

        public override IQueryable<TenancyProcess> GetQueryIncludes(IQueryable<TenancyProcess> query)
        {

            return query
                .Include(tp => tp.IdRentTypeNavigation)
                .Include(tp => tp.TenancyPersons)
                .Include(tp => tp.TenancyReasons);
        }

        public override IQueryable<TenancyProcess> GetQueryOrder(IQueryable<TenancyProcess> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdProcess")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdProcess);
                else
                    return query.OrderByDescending(p => p.IdProcess);
            }
            if (orderOptions.OrderField == "RegistrationNum")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.RegistrationNum == null ? p.ResidenceWarrantNum : p.RegistrationNum);
                else
                    return query.OrderByDescending(p => p.RegistrationNum == null ? p.ResidenceWarrantNum : p.RegistrationNum);
            }
            if (orderOptions.OrderField == "Address")
            {
                var addresses = tenancyBuildingsAssoc.Select(b => new
                {
                    b.IdProcess,
                    Address = string.Concat(b.BuildingNavigation.IdStreetNavigation.StreetName, ", ", b.BuildingNavigation.House)
                }).Union(tenancyPremisesAssoc.Select(
                    p => new
                    {
                        p.IdProcess,
                        Address = string.Concat(p.PremiseNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            p.PremiseNavigation.IdBuildingNavigation.House, ", ", p.PremiseNavigation.PremisesNum)
                    })
                ).Union(tenancySubPremisesAssoc.Select(
                    sp => new
                    {
                        sp.IdProcess,
                        Address = string.Concat(sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.PremisesNum, ", ", sp.SubPremiseNavigation.SubPremisesNum)
                    }));
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdProcess equals addr.IdProcess into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address
                            select row).Distinct();
                }
                else
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdProcess equals addr.IdProcess into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address descending
                            select row).Distinct();
                }
            }
            return query;
        }
    }
}
