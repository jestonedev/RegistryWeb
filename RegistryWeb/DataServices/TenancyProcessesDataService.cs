using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;

namespace RegistryWeb.DataServices
{
    public class TenancyProcessesDataService : ListDataService<TenancyProcessesVM, TenancyProcessesFilter>
    {
        private readonly IQueryable<TenancyBuildingAssoc> tenancyBuildingsAssoc;
        private readonly IQueryable<TenancyPremiseAssoc> tenancyPremisesAssoc;
        private readonly IQueryable<TenancySubPremiseAssoc> tenancySubPremisesAssoc;

        public TenancyProcessesDataService(RegistryContext registryContext) : base(registryContext)
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
        }

        public override TenancyProcessesVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, TenancyProcessesFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.ReasonTypes = registryContext.TenancyReasonTypes;
            viewModel.RentTypes = registryContext.RentTypes;
            viewModel.Streets = registryContext.KladrStreets;
            viewModel.OwnershipRightTypes = registryContext.OwnershipRightTypes;
            viewModel.ObjectStates = registryContext.ObjectStates;
            return viewModel;
        }

        internal TenancyProcessVM GetTenancyProcessViewModel(TenancyProcess process)
        {
            return new TenancyProcessVM
            {
                TenancyProcess = process,
                RentObjects = GetRentObjects(new List<TenancyProcess> { process }).SelectMany(r => r.Value).ToList(),
                Kinships = registryContext.Kinships.ToList(),
                RentTypeCategories = registryContext.RentTypeCategories.ToList(),
                RentTypes = registryContext.RentTypes.ToList(),
                TenancyReasonTypes = registryContext.TenancyReasonTypes.ToList(),
                Streets = registryContext.KladrStreets.ToList(),
                Executors = registryContext.Executors.ToList(),
                DocumentTypes = registryContext.DocumentTypes.ToList(),
                DocumentIssuedBy = registryContext.DocumentsIssuedBy.ToList()
            };
        }

        internal TenancyProcess GetTenancyProcess(int idProcess)
        {
            return registryContext.TenancyProcesses
                 .Include(tp => tp.TenancyPersons)
                 .Include(tp => tp.TenancyReasons)
                 .Include(tp => tp.TenancyBuildingsAssoc)
                 .Include(tp => tp.TenancyPremisesAssoc)
                 .Include(tp => tp.TenancySubPremisesAssoc)
                 .Include(tp => tp.TenancyRentPeriods)
                 .Include(tp => tp.TenancyAgreements)
                 .FirstOrDefault(tp => tp.IdProcess == idProcess);
        }

        internal TenancyProcessesVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            TenancyProcessesFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var tenancyProcesses = GetQuery();
            viewModel.PageOptions.TotalRows = tenancyProcesses.Count();
            var query = GetQueryFilter(tenancyProcesses, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.TenancyProcesses = query.ToList();
            viewModel.RentObjects = GetRentObjects(query);
            return viewModel;
        }

        private IQueryable<TenancyProcess> GetQuery()
        {
            return registryContext.TenancyProcesses;
        }

        private IQueryable<TenancyProcess> GetQueryIncludes(IQueryable<TenancyProcess> query)
        {
            return query
                .Include(tp => tp.IdRentTypeNavigation)
                .Include(tp => tp.TenancyPersons)
                .Include(tp => tp.TenancyReasons);
        }

        private Dictionary<int, List<TenancyRentObject>> GetRentObjects(IEnumerable<TenancyProcess> tenancyProcesses)
        {
            var buildings = from tbaRow in tenancyBuildingsAssoc
                            join tpRow in tenancyProcesses
                            on tbaRow.IdProcess equals tpRow.IdProcess
                            join buildingRow in registryContext.Buildings
                            on tbaRow.IdBuilding equals buildingRow.IdBuilding
                            join streetRow in registryContext.KladrStreets
                            on buildingRow.IdStreet equals streetRow.IdStreet
                            select new
                            {
                                tpRow.IdProcess,
                                RentObject = new TenancyRentObject
                                {
                                    Address = new Address
                                    {
                                        AddressType = AddressTypes.Building,
                                        Id = buildingRow.IdBuilding.ToString(),
                                        IdParents = new Dictionary<string, string>(),
                                        Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House)
                                    },
                                    TotalArea = buildingRow.TotalArea,
                                    LivingArea = buildingRow.LivingArea,
                                    RentArea = tbaRow.RentTotalArea
                                }
                            };
            var premises = from tpaRow in tenancyPremisesAssoc
                           join tpRow in tenancyProcesses
                           on tpaRow.IdProcess equals tpRow.IdProcess
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
                               tpRow.IdProcess,
                               RentObject = new TenancyRentObject
                               {
                                   Address = new Address
                                   {
                                       AddressType = AddressTypes.Premise,
                                       Id = premiseRow.IdPremises.ToString(),
                                       IdParents = new Dictionary<string, string>
                                       {
                                           { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() }
                                       },
                                       Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                        premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum)
                                   },
                                   TotalArea = premiseRow.TotalArea,
                                   LivingArea = premiseRow.LivingArea,
                                   RentArea = tpaRow.RentTotalArea
                               }
                           };
            var subPremises = from tspaRow in tenancySubPremisesAssoc
                              join tpRow in tenancyProcesses
                              on tspaRow.IdProcess equals tpRow.IdProcess
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
                                  tpRow.IdProcess,
                                  RentObject = new TenancyRentObject
                                  {
                                      Address = new Address
                                      {
                                          AddressType = AddressTypes.SubPremise,
                                          Id = subPremiseRow.IdSubPremises.ToString(),
                                          IdParents = new Dictionary<string, string>
                                           {
                                                { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() },
                                                { AddressTypes.Premise.ToString(), premiseRow.IdPremises.ToString() }
                                           },
                                          Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                            premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum, ", к." + subPremiseRow.SubPremisesNum)
                                      },
                                      TotalArea = subPremiseRow.TotalArea,
                                      LivingArea = subPremiseRow.LivingArea,
                                      RentArea = tspaRow.RentTotalArea
                                  }
                              };

            var objects = buildings.Union(premises).Union(subPremises).ToList();

            var payments = (from paymentsRow in registryContext.TenancyPayments
                            join tpRow in tenancyProcesses
                            on paymentsRow.IdProcess equals tpRow.IdProcess
                            where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                tpRow.TenancyPersons.Any()
                            select paymentsRow).ToList();

            var paymentsAfter28082019Buildings = (from tbaRow in tenancyBuildingsAssoc
                                                  join tpRow in tenancyProcesses
                                                  on tbaRow.IdProcess equals tpRow.IdProcess
                                                  join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                  on tbaRow.IdBuilding equals paymentRow.IdBuilding
                                                  where paymentRow.IdPremises == null &&
                                                    (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                    tpRow.TenancyPersons.Any()
                                                  select paymentRow).Distinct().ToList();
            var paymentsAfter28082019Premises = (from tpaRow in tenancyPremisesAssoc
                                                 join tpRow in tenancyProcesses
                                                 on tpaRow.IdProcess equals tpRow.IdProcess
                                                 join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                 on tpaRow.IdPremise equals paymentRow.IdPremises
                                                 where paymentRow.IdSubPremises == null &&
                                                    (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                    tpRow.TenancyPersons.Any()
                                                 select paymentRow).Distinct().ToList();
            var paymentsAfter28082019SubPremises = (from tspaRow in tenancySubPremisesAssoc
                                                    join tpRow in tenancyProcesses
                                                    on tspaRow.IdProcess equals tpRow.IdProcess
                                                    join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tspaRow.IdSubPremise equals paymentRow.IdSubPremises
                                                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                        tpRow.TenancyPersons.Any()
                                                    select paymentRow).Distinct().ToList();

            foreach(var obj in objects)
            {
                if (obj.RentObject.Address.AddressType == AddressTypes.Building)
                {
                    obj.RentObject.Payment = 
                        payments.Where(r => r.IdBuilding.ToString() == obj.RentObject.Address.Id && r.IdPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                       Math.Round(paymentsAfter28082019Buildings.Where(
                            r => r.IdBuilding.ToString() == obj.RentObject.Address.Id && r.IdPremises == null
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.Premise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdPremises.ToString() == obj.RentObject.Address.Id && r.IdSubPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019Premises.Where(
                            r => r.IdPremises.ToString() == obj.RentObject.Address.Id && r.IdSubPremises == null
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.SubPremise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdSubPremises.ToString() == obj.RentObject.Address.Id).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019SubPremises.Where(
                            r => r.IdSubPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
            }

            var result = 
                objects.GroupBy(r => r.IdProcess)
                .Select(r => new { IdProcess = r.Key, RentObject = r.Select(v => v.RentObject) })
                .ToDictionary(v => v.IdProcess, v => v.RentObject.ToList());
            return result;
        }

        private IQueryable<TenancyProcess> GetQueryFilter(IQueryable<TenancyProcess> query, TenancyProcessesFilter filterOptions)
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
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                var idBuildingProcesses = tenancyBuildingsAssoc
                    .Where(oba => oba.BuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = tenancyPremisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
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
                var idBuildingProcesses = tenancyBuildingsAssoc
                    .Where(oba => oba.IdBuilding == id)
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = tenancyPremisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuilding == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding == id)
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
                    .Where(opa => opa.PremiseNavigation.IdPremises == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises == id)
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
                    .Where(ospa => ospa.SubPremiseNavigation.IdSubPremises == id)
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
                query = query.Where(p => p.RegistrationDate == filterOptions.RegistrationDate);
            }
            if (filterOptions.IssuedDate.HasValue)
            {
                query = query.Where(p => p.IssueDate == filterOptions.IssuedDate);
            }
            if (filterOptions.BeginDate.HasValue)
            {
                query = query.Where(p => p.BeginDate == filterOptions.BeginDate);
            }
            if (filterOptions.EndDate.HasValue)
            {
                query = query.Where(p => p.EndDate == filterOptions.EndDate);
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
                var tenantSnp = string.IsNullOrEmpty(filterOptions.TenantSnp) ? null : filterOptions.TenantSnp;
                var tenancyParticipantSnp = string.IsNullOrEmpty(filterOptions.TenancyParticipantSnp) ? null : filterOptions.TenancyParticipantSnp;
                query = (from tRow in query
                         join tpRow in registryContext.TenancyPersons
                         on tRow.IdProcess equals tpRow.IdProcess
                         where tpRow.ExcludeDate == null &&
                             ((tenantSnp != null && tpRow.IdKinship == 1 &&
                                 string.Concat(tpRow.Surname, " ", tpRow.Name, " ", tpRow.Patronymic).Contains(tenantSnp)) ||
                             (tenancyParticipantSnp != null &&
                                 string.Concat(tpRow.Surname, " ", tpRow.Name, " ", tpRow.Patronymic).Contains(tenancyParticipantSnp)))
                         select tRow).Distinct();
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
                               streetRow.IdStreet,
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
                                  streetRow.IdStreet,
                                  buildingRow.House,
                                  premiseRow.PremisesNum,
                                  subPremiseRow.SubPremisesNum,
                                  subPremiseRow.IdState
                              };
            IEnumerable<int> idsProcess = null;

            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var ids = buildings.Where(r => r.IdStreet == filterOptions.IdStreet).Select(r => r.IdProcess)
                    .Union(premises.Where(r => r.IdStreet == filterOptions.IdStreet).Select(r => r.IdProcess))
                    .Union(subPremises.Where(r => r.IdStreet == filterOptions.IdStreet).Select(r => r.IdProcess));
                idsProcess = ids;
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var ids = buildings.Where(r => r.House == filterOptions.House).Select(r => r.IdProcess)
                    .Union(premises.Where(r => r.House == filterOptions.House).Select(r => r.IdProcess))
                    .Union(subPremises.Where(r => r.House == filterOptions.House).Select(r => r.IdProcess));
                if (idsProcess == null)
                {
                    idsProcess = ids;
                } else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var ids = premises.Where(r => r.PremisesNum == filterOptions.PremisesNum).Select(r => r.IdProcess)
                    .Union(subPremises.Where(r => r.PremisesNum == filterOptions.PremisesNum).Select(r => r.IdProcess));
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
                    .Union(subPremises.Where(r => filterOptions.IdsObjectState.Any(s => s == r.IdState)).Select(r => r.IdProcess));
                query = (from row in query
                         join id in idsProcess
                         on row.IdProcess equals id
                         select row).Distinct();
            }
            if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any())
            {
                // TODO
            }
            return query;
        }

        private IQueryable<TenancyProcess> GetQueryOrder(IQueryable<TenancyProcess> query, OrderOptions orderOptions)
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
                } else
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

        private IQueryable<TenancyProcess> GetQueryPage(IQueryable<TenancyProcess> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }
    }
}