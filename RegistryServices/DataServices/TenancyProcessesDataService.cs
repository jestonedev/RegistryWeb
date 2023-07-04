using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;
using System.Text.RegularExpressions;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.Tenancies;
using RegistryServices.ViewModel.Payments;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryWeb.DataServices
{
    public class TenancyProcessesDataService : ListDataService<TenancyProcessesVM, TenancyProcessesFilter>
    {
        private readonly IQueryable<TenancyBuildingAssoc> tenancyBuildingsAssoc;
        private readonly IQueryable<TenancyPremiseAssoc> tenancyPremisesAssoc;
        private readonly IQueryable<TenancySubPremiseAssoc> tenancySubPremisesAssoc;
        private readonly SecurityService securityService;
        private readonly IConfiguration config;

        public TenancyProcessesDataService(RegistryContext registryContext, SecurityService securityService,
            AddressesDataService addressesDataService, IConfiguration config) : base(registryContext, addressesDataService)
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
            this.securityService = securityService;
            this.config = config;
        }

        public override TenancyProcessesVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, TenancyProcessesFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.ReasonTypes = registryContext.TenancyReasonTypes;
            viewModel.RentTypes = registryContext.RentTypes;
            viewModel.Regions = addressesDataService.KladrRegions;
            viewModel.Streets = addressesDataService.KladrStreets;
            viewModel.OwnershipRightTypes = registryContext.OwnershipRightTypes;
            viewModel.ObjectStates = registryContext.ObjectStates;
            return viewModel;
        }

        public TenancyProcessVM CreateTenancyProcessEmptyViewModel(int? idObject = null, 
            AddressTypes addressType = AddressTypes.None,
            [CallerMemberName]string action = "")
        {
            var userName = securityService.User.UserName.ToLowerInvariant();

            var rentObjects = new List<TenancyRentObject>();

            switch(addressType)
            {
                case AddressTypes.Building:
                    var building = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == idObject);
                    rentObjects.Add(new TenancyRentObject
                    {
                        Address = new Address
                        {
                            AddressType = addressType,
                            Id = (building?.IdBuilding ?? 0).ToString(),
                            IdParents = new Dictionary<string, string> {
                                { AddressTypes.Street.ToString(), building?.IdStreet }
                            }
                        },
                        TotalArea = building?.TotalArea ?? 0,
                        LivingArea = building?.LivingArea ?? 0
                    });
                    break;
                case AddressTypes.Premise:
                    var premise = registryContext.Premises.Include(b => b.IdBuildingNavigation)
                        .FirstOrDefault(p => p.IdPremises == idObject);
                    rentObjects.Add(new TenancyRentObject
                    {
                        Address = new Address
                        {
                            AddressType = addressType,
                            Id = (premise?.IdPremises ?? 0).ToString(),
                            IdParents = new Dictionary<string, string> {
                                { AddressTypes.Street.ToString(), premise?.IdBuildingNavigation.IdStreet },
                                { AddressTypes.Building.ToString(), premise?.IdBuilding.ToString() }
                            }
                        },
                        TotalArea = premise?.TotalArea ?? 0,
                        LivingArea = premise?.LivingArea ?? 0
                    });
                    break;
                case AddressTypes.SubPremise:
                    var subPremise = registryContext.SubPremises.Include(p => p.IdPremisesNavigation)
                                .ThenInclude(b => b.IdBuildingNavigation)
                                .FirstOrDefault(sp => sp.IdSubPremises == idObject);
                    rentObjects.Add(new TenancyRentObject
                    {
                        Address = new Address
                        {
                            AddressType = addressType,
                            Id = (subPremise?.IdSubPremises ?? 0).ToString(),
                            IdParents = new Dictionary<string, string> {
                                { AddressTypes.Street.ToString(), subPremise?.IdPremisesNavigation.IdBuildingNavigation.IdStreet },
                                { AddressTypes.Building.ToString(), subPremise?.IdPremisesNavigation.IdBuilding.ToString() },
                                { AddressTypes.Premise.ToString(), subPremise?.IdPremises.ToString() }
                            }
                        },
                        TotalArea = subPremise?.TotalArea ?? 0,
                        LivingArea = subPremise?.LivingArea ?? 0
                    });
                    break;
            }
            var registryNums = registryContext.TenancyProcesses.Where(r => r.RegistrationNum != null)
                .Select(r => r.RegistrationNum).ToList().Where(r => Regex.IsMatch(r, "^[0-9]+/[0-9]{1,2}/" + (DateTime.Now.Year % 100) + "н?$"));
            var registryNumLast = 0;
            if (registryNums.Any())
                registryNumLast = registryNums.Select(r => int.Parse(r.Split("/")[0])).Max();
            return new TenancyProcessVM
            {
                TenancyProcess = new TenancyProcess {
                    RegistrationNum = string.Format("{0}/{1}/{2}", 
                        registryNumLast+1, 
                        DateTime.Now.Month.ToString().PadLeft(2, '0'), 
                        (DateTime.Now.Year%100).ToString().PadLeft(2, '0')
                    )
                },                Kinships = registryContext.Kinships.ToList(),
                RentTypeCategories = registryContext.RentTypeCategories.ToList(),
                RentTypes = registryContext.RentTypes.ToList(),
                TenancyReasonTypes = registryContext.TenancyReasonTypes.ToList(),
                Streets = registryContext.KladrStreets.ToList(),
                Executors = (action == "Details" || action == "Delete") ? registryContext.Executors.ToList() : registryContext.Executors.Where(e => !e.IsInactive).ToList(),
                CurrentExecutor = registryContext.Executors.FirstOrDefault(e => e.ExecutorLogin != null && 
                        e.ExecutorLogin.ToLowerInvariant() == userName),
                DocumentTypes = registryContext.DocumentTypes.ToList(),
                DocumentIssuedBy = registryContext.DocumentsIssuedBy.ToList(),
                TenancyProlongRentReasons = registryContext.TenancyProlongRentReasons.ToList(),
                Employers = registryContext.Employers.ToList(),
                RentObjects = rentObjects
            };
        }

        public TenancyProcessVM ClearifyTenancyProcessVmForCopy(TenancyProcessVM tenancyProcessVM)
        {
            var tenancyProcess = tenancyProcessVM.TenancyProcess;
            tenancyProcess.IdProcess = 0;
            tenancyProcess.ProtocolDate = null;
            tenancyProcess.ProtocolNum = null;
            tenancyProcess.IdExecutor = registryContext.Executors.FirstOrDefault(e => e.ExecutorLogin != null &&
                        e.ExecutorLogin.ToLowerInvariant() == securityService.User.UserName.ToLowerInvariant())?.IdExecutor;
            foreach (var person in tenancyProcess.TenancyPersons)
            {
                person.IdProcess = 0;
                person.IdPerson = 0;
            }
            foreach (var building in tenancyProcess.TenancyBuildingsAssoc)
            {
                building.IdProcess = 0;
            }
            foreach (var premise in tenancyProcess.TenancyPremisesAssoc)
            {
                premise.IdProcess = 0;
            }
            foreach (var subPremise in tenancyProcess.TenancySubPremisesAssoc)
            {
                subPremise.IdProcess = 0;
            }

            foreach (var accountAssoc in tenancyProcess.AccountsTenancyProcessesAssoc)
            {
                accountAssoc.IdAssoc = 0;
            }
            tenancyProcess.TenancyReasons = null;
            tenancyProcess.TenancyAgreements = null;
            tenancyProcess.TenancyFiles = null;
            tenancyProcess.TenancyRentPeriods = null;
            return tenancyProcessVM;
        }

        public TenancyProcessVM GetTenancyProcessViewModel(TenancyProcess process, [CallerMemberName]string action = "")
        {
            var tenancyProcessVM = CreateTenancyProcessEmptyViewModel(null, AddressTypes.None, action);
            tenancyProcessVM.TenancyProcess = process;
            tenancyProcessVM.RentObjects = GetRentObjects(new List<TenancyProcess> { process }).SelectMany(r => r.Value).ToList();
            return tenancyProcessVM;
        }

        public TenancyProcess GetTenancyProcess(int idProcess)
        {
            var tenancyProcess = registryContext.TenancyProcesses
                 .Include(tp => tp.TenancyPersons)
                 .Include(tp => tp.TenancyReasons)
                 .Include(tp => tp.TenancyBuildingsAssoc)
                 .Include(tp => tp.TenancyPremisesAssoc)
                 .Include(tp => tp.TenancySubPremisesAssoc)
                 .Include(tp => tp.TenancyRentPeriods)
                 .Include(tp => tp.TenancyAgreements)
                 .Include(tp => tp.TenancyFiles)
                 .Include(tp => tp.AccountsTenancyProcessesAssoc)
                 .FirstOrDefault(tp => tp.IdProcess == idProcess);
            foreach(var assoc in tenancyProcess.AccountsTenancyProcessesAssoc)
            {
                assoc.AccountNavigation = registryContext.KumiAccounts.FirstOrDefault(r => r.IdAccount == assoc.IdAccount);
            }
            return tenancyProcess;
        }

        public TenancyProcessesVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            TenancyProcessesFilter filterOptions, out List<int> filteredIds)
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

            filteredIds = query.Select(p => p.IdProcess).ToList();

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.TenancyProcesses = query.ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.TenancyProcesses);
            viewModel.AreaAvgCostActualDate = registryContext.TotalAreaAvgCosts.FirstOrDefault()?.Date;
            return viewModel;
        }

        public IQueryable<TenancyProcess> GetTenancyProcesses(TenancyProcessesFilter filterOptions)
        {
            var tenancyProcesses = GetQuery();
            var query = GetQueryFilter(tenancyProcesses, filterOptions);
            query = query.Include(r => r.AccountsTenancyProcessesAssoc).Include(r => r.TenancyPersons);
            return query;
        }

        private IQueryable<TenancyProcess> GetQuery()
        {
            return registryContext.TenancyProcesses.Include(r => r.AccountsTenancyProcessesAssoc);
        }

        private IQueryable<TenancyProcess> GetQueryIncludes(IQueryable<TenancyProcess> query)
        {
            return query
                .Include(tp => tp.IdRentTypeNavigation)
                .Include(tp => tp.TenancyPersons)
                .Include(tp => tp.TenancyReasons);
        }

        public Dictionary<int, List<TenancyRentObject>> GetRentObjects(IEnumerable<TenancyProcess> tenancyProcesses)
        {
            var ids = tenancyProcesses.Select(r => r.IdProcess);
            var buildings = from tbaRow in tenancyBuildingsAssoc
                            join buildingRow in registryContext.Buildings
                            on tbaRow.IdBuilding equals buildingRow.IdBuilding
                            join streetRow in registryContext.KladrStreets
                            on buildingRow.IdStreet equals streetRow.IdStreet
                            where ids.Contains(tbaRow.IdProcess)
                            select new
                            {
                                tbaRow.IdProcess,
                                RentObject = new TenancyRentObject
                                {
                                    Address = new Address
                                    {
                                        AddressType = AddressTypes.Building,
                                        Id = buildingRow.IdBuilding.ToString(),
                                        IdParents = new Dictionary<string, string> {
                                            { AddressTypes.Street.ToString(), buildingRow.IdStreet }
                                        },
                                        Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House)
                                    },
                                    TotalArea = buildingRow.TotalArea,
                                    LivingArea = buildingRow.LivingArea,
                                    RentArea = tbaRow.RentTotalArea
                                }
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
                           where ids.Contains(tpaRow.IdProcess)
                           select new
                           {
                               tpaRow.IdProcess,
                               RentObject = new TenancyRentObject
                               {
                                   Address = new Address
                                   {
                                       AddressType = AddressTypes.Premise,
                                       Id = premiseRow.IdPremises.ToString(),
                                       IdParents = new Dictionary<string, string>
                                       {
                                           { AddressTypes.Street.ToString(), buildingRow.IdStreet },
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
                              where ids.Contains(tspaRow.IdProcess)
                              select new
                              {
                                  tspaRow.IdProcess,
                                  RentObject = new TenancyRentObject
                                  {
                                      Address = new Address
                                      {
                                          AddressType = AddressTypes.SubPremise,
                                          Id = subPremiseRow.IdSubPremises.ToString(),
                                          IdParents = new Dictionary<string, string>
                                           {
                                              { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                              { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() },
                                              { AddressTypes.Premise.ToString(), premiseRow.IdPremises.ToString() }
                                           },
                                          Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                            premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum, ", к.", subPremiseRow.SubPremisesNum)
                                      },
                                      TotalArea = subPremiseRow.TotalArea,
                                      LivingArea = subPremiseRow.LivingArea,
                                      RentArea = tspaRow.RentTotalArea
                                  }
                              };

            var objects = buildings.Union(premises).Union(subPremises).ToList();

            var payments = (from paymentsRow in registryContext.TenancyPayments
                            where ids.Contains(paymentsRow.IdProcess)
                            select paymentsRow).ToList();

            payments = (from paymentRow in payments
                            join tpRow in tenancyProcesses
                            on paymentRow.IdProcess equals tpRow.IdProcess
                            where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                tpRow.TenancyPersons.Any()
                            select paymentRow).ToList();

            var prePaymentsAfter28082019Buildings = (from tbaRow in tenancyBuildingsAssoc
                                                     join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                     on tbaRow.IdBuilding equals paymentRow.IdBuilding
                                                     where paymentRow.IdPremises == null && ids.Contains(tbaRow.IdProcess)
                                                     select new
                                                     {
                                                         tbaRow.IdProcess,
                                                         paymentRow.IdBuilding,
                                                         paymentRow.Hb,
                                                         paymentRow.K1,
                                                         paymentRow.K2,
                                                         paymentRow.K3,
                                                         paymentRow.KC,
                                                         RentArea = tbaRow.RentTotalArea == null ? paymentRow.RentArea : tbaRow.RentTotalArea
                                                     }).Distinct().ToList();

            var paymentsAfter28082019Buildings = (from paymentRow in prePaymentsAfter28082019Buildings
                                              join tpRow in tenancyProcesses
                                                  on paymentRow.IdProcess equals tpRow.IdProcess
                                                  where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                    tpRow.TenancyPersons.Any()
                                                  select paymentRow).Distinct().ToList();

            var prePaymentsAfter28082019Premises = (from tpaRow in tenancyPremisesAssoc
                                                     join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                     on tpaRow.IdPremise equals paymentRow.IdPremises
                                                    where paymentRow.IdSubPremises == null && ids.Contains(tpaRow.IdProcess)
                                                     select new
                                                     {
                                                         tpaRow.IdProcess,
                                                         paymentRow.IdPremises,
                                                         paymentRow.Hb,
                                                         paymentRow.K1,
                                                         paymentRow.K2,
                                                         paymentRow.K3,
                                                         paymentRow.KC,
                                                         RentArea = tpaRow.RentTotalArea == null ? paymentRow.RentArea : tpaRow.RentTotalArea
                                                     }).Distinct().ToList();

            var paymentsAfter28082019Premises = (from paymentRow in prePaymentsAfter28082019Premises
                                                 join tpRow in tenancyProcesses
                                                     on paymentRow.IdProcess equals tpRow.IdProcess
                                                 where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                   tpRow.TenancyPersons.Any()
                                                 select paymentRow).Distinct().ToList();

            var prePaymentsAfter28082019SubPremises = (from tspaRow in tenancySubPremisesAssoc
                                                       join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tspaRow.IdSubPremise equals paymentRow.IdSubPremises
                                                       where ids.Contains(tspaRow.IdProcess)
                                                    select new
                                                    {
                                                        tspaRow.IdProcess,
                                                        paymentRow.IdSubPremises,
                                                        paymentRow.Hb,
                                                        paymentRow.K1,
                                                        paymentRow.K2,
                                                        paymentRow.K3,
                                                        paymentRow.KC,
                                                        RentArea = tspaRow.RentTotalArea == null ? paymentRow.RentArea : tspaRow.RentTotalArea
                                                    }).Distinct().ToList();



            var paymentsAfter28082019SubPremises = (from paymentRow in prePaymentsAfter28082019SubPremises
                                                    join tpRow in tenancyProcesses
                                                        on paymentRow.IdProcess equals tpRow.IdProcess
                                                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                      tpRow.TenancyPersons.Any()
                                                    select paymentRow).Distinct().ToList();

            foreach(var obj in objects)
            {
                if (obj.RentObject.Address.AddressType == AddressTypes.Building)
                {
                    obj.RentObject.Payment = 
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdBuilding.ToString() == obj.RentObject.Address.Id && r.IdPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                       Math.Round(paymentsAfter28082019Buildings.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdBuilding.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.Premise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdPremises.ToString() == obj.RentObject.Address.Id && r.IdSubPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019Premises.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.SubPremise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdSubPremises.ToString() == obj.RentObject.Address.Id).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019SubPremises.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdSubPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
            }

            var result = 
                objects.GroupBy(r => r.IdProcess)
                .Select(r => new { IdProcess = r.Key, RentObject = r.Select(v => v.RentObject) })
                .ToDictionary(v => v.IdProcess, v => v.RentObject.ToList());
            return result;
        }

		public void AddEmployer(Employer employer)
        {
            registryContext.Employers.Add(employer);
            registryContext.SaveChanges();
        }

        public Employer GetEmploeyerByName(string employerName)
        {
            return registryContext.Employers.FirstOrDefault(r => r.EmployerName == employerName);
        }

        public List<PaymentHistoryVM> GetPaymentHistory(int id, PaymentHistoryTarget target)        {
            switch(target)
            {
                case PaymentHistoryTarget.SubPremise:
                    var payments = (from paymentRow in registryContext.TenancyPaymentsHistory
                                    where paymentRow.IdSubPremises != null && paymentRow.IdSubPremises == id
                                    select paymentRow).ToList();

                    var subPremisesIds = payments.Select(p => p.IdSubPremises).Distinct();

                    var activeTenanciesSubPremisesIds = 
                        (from tpRow in registryContext.TenancyProcesses
                        join personRow in registryContext.TenancyPersons
                        on tpRow.IdProcess equals personRow.IdProcess
                        join assocRow in registryContext.TenancySubPremisesAssoc
                        on tpRow.IdProcess equals assocRow.IdProcess
                        where subPremisesIds.Contains(assocRow.IdSubPremise) && (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н"))
                        select assocRow.IdSubPremise).Distinct().ToList();
                    return (from paymentRow in payments
                            join subPremiseRow in registryContext.SubPremises.Where(r => subPremisesIds.Contains(r.IdSubPremises))
                            on paymentRow.IdSubPremises equals subPremiseRow.IdSubPremises
                            where activeTenanciesSubPremisesIds.Contains(subPremiseRow.IdSubPremises)
                            select new PaymentHistoryVM
                            {
                                TenancyPaymentHistory = paymentRow,
                                ObjectDescription = "Комната " + subPremiseRow.SubPremisesNum
                            }).ToList();
                case PaymentHistoryTarget.Premise:
                    var paymentsSubPremises = (from paymentRow in registryContext.TenancyPaymentsHistory
                                    where paymentRow.IdSubPremises != null && paymentRow.IdPremises == id
                                    select paymentRow).ToList();

                    subPremisesIds = paymentsSubPremises.Select(p => p.IdSubPremises).Distinct();

                    activeTenanciesSubPremisesIds =
                        (from tpRow in registryContext.TenancyProcesses
                         join personRow in registryContext.TenancyPersons
                         on tpRow.IdProcess equals personRow.IdProcess
                         join assocRow in registryContext.TenancySubPremisesAssoc
                         on tpRow.IdProcess equals assocRow.IdProcess
                         where subPremisesIds.Contains(assocRow.IdSubPremise) && (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н"))
                         select assocRow.IdSubPremise).Distinct().ToList();

                    var paymentsPremises = (from paymentRow in registryContext.TenancyPaymentsHistory
                                               where paymentRow.IdSubPremises == null && paymentRow.IdPremises == id
                                               select paymentRow).ToList();

                    var premisesIds = paymentsPremises.Select(p => p.IdPremises).Distinct();

                    var activeTenanciesPremisesIds =
                        (from tpRow in registryContext.TenancyProcesses
                         join personRow in registryContext.TenancyPersons
                         on tpRow.IdProcess equals personRow.IdProcess
                         join assocRow in registryContext.TenancyPremisesAssoc
                         on tpRow.IdProcess equals assocRow.IdProcess
                         where premisesIds.Contains(assocRow.IdPremise) && (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н"))
                         select assocRow.IdPremise).Distinct().ToList();
                    
                    return (from paymentRow in paymentsPremises
                            where paymentRow.IdPremises != null && activeTenanciesPremisesIds.Contains(paymentRow.IdPremises.Value)
                            select new PaymentHistoryVM
                            {
                                TenancyPaymentHistory = paymentRow,
                                ObjectDescription = "Квартира"
                            }).Union(
                            from paymentRow in paymentsSubPremises
                            join subPremiseRow in registryContext.SubPremises.Where(r => subPremisesIds.Contains(r.IdSubPremises))
                            on paymentRow.IdSubPremises equals subPremiseRow.IdSubPremises
                            where activeTenanciesSubPremisesIds.Contains(subPremiseRow.IdSubPremises)
                            select new PaymentHistoryVM
                            {
                                TenancyPaymentHistory = paymentRow,
                                ObjectDescription = "Комната №" + subPremiseRow.SubPremisesNum
                            }).ToList();
                case PaymentHistoryTarget.Tenancy:
                    var process = registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons).Where(r => r.IdProcess == id).AsNoTracking().FirstOrDefault();
                    var rentObjects = GetRentObjects(registryContext.TenancyProcesses.Where(r => r.IdProcess == id).AsNoTracking().ToList()).SelectMany(v => v.Value);
                    premisesIds = rentObjects.Where(r => r.Address.AddressType == AddressTypes.Premise).Select(r => (int?)int.Parse(r.Address.Id)).ToList();
                    paymentsPremises = (from paymentRow in registryContext.TenancyPaymentsHistory
                                            where paymentRow.IdSubPremises == null && premisesIds.Contains(paymentRow.IdPremises)
                                            select paymentRow).ToList();

                    subPremisesIds = rentObjects.Where(r => r.Address.AddressType == AddressTypes.SubPremise).Select(r => (int?)int.Parse(r.Address.Id)).ToList();
                    paymentsSubPremises = (from paymentRow in registryContext.TenancyPaymentsHistory
                                           where paymentRow.IdSubPremises != null && subPremisesIds.Contains(paymentRow.IdSubPremises)
                                           select paymentRow).ToList();
                    var result = (from paymentRow in paymentsPremises
                            select new PaymentHistoryVM
                            {
                                TenancyPaymentHistory = paymentRow,
                                ObjectDescription =
                                    rentObjects.Where(r => r.Address.AddressType == AddressTypes.Premise && r.Address.Id == paymentRow.IdPremises.ToString())
                                        .Select(r => r.Address.Text).FirstOrDefault()
                            }).Union(
                            from paymentRow in paymentsSubPremises
                            select new PaymentHistoryVM
                            {
                                TenancyPaymentHistory = paymentRow,
                                ObjectDescription =
                                    rentObjects.Where(r => r.Address.AddressType == AddressTypes.SubPremise && r.Address.Id == paymentRow.IdSubPremises.ToString())
                                        .Select(r => r.Address.Text).FirstOrDefault()
                            }
                        ).ToList();
                    if ((process.RegistrationNum != null && process.RegistrationNum.EndsWith("н")) || !process.TenancyPersons.Any())
                    {
                        result.Add(new PaymentHistoryVM {
                            ObjectDescription = "Плата по процессу найма отсутствиет в связи с аннулированием договора"
                        });
                    }
                    return result;
            }
            return null;
        }

        public string GetPaymentHistoryTitle(int id, PaymentHistoryTarget target)
        {
            switch(target)
            {
                case PaymentHistoryTarget.Premise:
                    return string.Format("по помещению №{0}", id);
                case PaymentHistoryTarget.SubPremise:
                    var subPremise = registryContext.SubPremises.FirstOrDefault(sp => sp.IdSubPremises == id);
                    if (subPremise == null) throw new Exception(string.Format("Не удалось найти комнату с идентификатором {0}", id));
                    return string.Format("по комнате №{0} помещения №{1}", subPremise.SubPremisesNum, id);
                case PaymentHistoryTarget.Tenancy:
                    return string.Format("по объектам найма №{0}", id);
            }
            throw new Exception("Некорректный тип целевого объекта для выборки истории найма");
        }

        public void UpdateExcludeDate(int? idProcess, DateTime? beginDate, DateTime? endDate, bool untilDismissal)
        {
            var process = registryContext.TenancyProcesses.FirstOrDefault(tp => tp.IdProcess == idProcess);
            process.BeginDate = beginDate;
            process.EndDate = endDate;
            process.UntilDismissal = untilDismissal;
            registryContext.SaveChanges();
        }

        public void Create(TenancyProcess tenancyProcess, IList<TenancyRentObject> rentObjects, List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            if (tenancyProcess.TenancyReasons != null)
            {
                foreach(var reason in tenancyProcess.TenancyReasons)
                {
                    var tenancyReasonType = registryContext.TenancyReasonTypes.FirstOrDefault(tr => tr.IdReasonType == reason.IdReasonType);
                    if (tenancyReasonType == null)
                        throw new Exception("Некорректный тип основания найма");
                    reason.ReasonPrepared = tenancyReasonType.ReasonTemplate
                        .Replace("@reason_date@", reason.ReasonDate.HasValue ? reason.ReasonDate.Value.ToString("dd.MM.yyyy") : "")
                        .Replace("@reason_number@", reason.ReasonNumber);
                }
            }
            if (rentObjects != null)
            {
                foreach (var rentObject in rentObjects)
                {
                    switch(rentObject.Address.AddressType)
                    {
                        case AddressTypes.Building:
                            tenancyProcess.TenancyBuildingsAssoc.Add(new TenancyBuildingAssoc
                            {
                                IdBuilding = int.Parse(rentObject.Address.Id),
                                RentTotalArea = rentObject.RentArea
                            });
                            break;
                        case AddressTypes.Premise:
                            tenancyProcess.TenancyPremisesAssoc.Add(new TenancyPremiseAssoc
                            {
                                IdPremise = int.Parse(rentObject.Address.Id),
                                RentTotalArea = rentObject.RentArea
                            });
                            break;
                        case AddressTypes.SubPremise:
                            tenancyProcess.TenancySubPremisesAssoc.Add(new TenancySubPremiseAssoc
                            {
                                IdSubPremise = int.Parse(rentObject.Address.Id),
                                RentTotalArea = rentObject.RentArea
                            });
                            break;
                    }
                }
            }

            // Прикрепляем документы
            var tenancyFilesPath = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Tenancies\");
            if (tenancyProcess.TenancyFiles != null)
            {
                for (var i = 0; i < tenancyProcess.TenancyFiles.Count; i++)
                {
                    tenancyProcess.TenancyFiles[i].FileName = "";
                    var file = files.Where(r => r.Name == "TenancyFile[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    tenancyProcess.TenancyFiles[i].DisplayName = file.FileName;
                    tenancyProcess.TenancyFiles[i].FileName = Guid.NewGuid().ToString() + "." + new FileInfo(file.FileName).Extension;
                    tenancyProcess.TenancyFiles[i].MimeType = file.ContentType;
                    var fileStream = new FileStream(Path.Combine(tenancyFilesPath, tenancyProcess.TenancyFiles[i].FileName), FileMode.CreateNew);
                    file.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            var assocs = tenancyProcess.AccountsTenancyProcessesAssoc;
            tenancyProcess.AccountsTenancyProcessesAssoc = null;
            registryContext.TenancyProcesses.Add(tenancyProcess);
            registryContext.SaveChanges();
            if (!assocs.Any(r => r.IdAccount != 0 || !string.IsNullOrEmpty(r.AccountNavigation.Account)))
            {
                assocs = FindAccountFromPrevTenancyOrCreateNew(assocs, tenancyProcess);
            }
            AddAndBindAccounts(assocs, tenancyProcess.IdProcess);
            registryContext.SaveChanges();
        }

        private IList<KumiAccountsTenancyProcessesAssoc> FindAccountFromPrevTenancyOrCreateNew(IList<KumiAccountsTenancyProcessesAssoc> accountsAssoc, TenancyProcess tenancyProcess)
        {
            var tenancies = tenancyProcess.TenancyPersons.Select(r => r.Surname.Trim() + " " + r.Name.Trim() + (r.Patronymic != null ? (" " + r.Patronymic.Trim()) : ""));

            var addressInfix = (from row in
                                (from row in tenancyProcess.TenancyBuildingsAssoc
                                 select new
                                 {
                                     row.IdProcess,
                                     Infix = string.Concat("b", row.IdBuilding)
                                 })
                                .Union(from row in tenancyProcess.TenancyPremisesAssoc
                                       select new
                                       {
                                           row.IdProcess,
                                           Infix = string.Concat("p", row.IdPremise)
                                       })
                                .Union(from row in tenancyProcess.TenancySubPremisesAssoc
                                       select new
                                       {
                                           row.IdProcess,
                                           Infix = string.Concat("sp", row.IdSubPremise)
                                       })
                                 orderby row.Infix
                                 group row.Infix by row.IdProcess into gs
                                 select new
                                 {
                                     IdProcess = gs.Key,
                                     AddressCode = string.Join("", gs)
                                 }).Select(r => r.AddressCode).FirstOrDefault();

            var tenancyIdsByTenant = registryContext.TenancyPersons.Where(r => tenancies.Contains(r.Surname.Trim() + " " + r.Name.Trim() + (r.Patronymic != null ? (" " + r.Patronymic.Trim()) : ""))).Select(r => r.IdProcess).ToList().Distinct();

            var allObjects = (from row in
                                (from row in registryContext.TenancyBuildingsAssoc
                                 select new
                                 {
                                     row.IdProcess,
                                     Infix = string.Concat("b", row.IdBuilding)
                                 })
                                .Union(from row in registryContext.TenancyPremisesAssoc
                                       select new
                                       {
                                           row.IdProcess,
                                           Infix = string.Concat("p", row.IdPremise)
                                       })
                                .Union(from row in registryContext.TenancySubPremisesAssoc
                                       select new
                                       {
                                           row.IdProcess,
                                           Infix = string.Concat("sp", row.IdSubPremise)
                                       })
                              orderby row.Infix
                              group row.Infix by row.IdProcess into gs
                              select new
                              {
                                  IdProcess = gs.Key,
                                  AddressCode = string.Join("", gs)
                              }).AsEnumerable();

            var tenancyIdsByAddress = (from allObjectsRow in allObjects
                                    where allObjectsRow.AddressCode == addressInfix
                                       select allObjectsRow.IdProcess).ToList();

            var accounts = registryContext.KumiAccountsTenancyProcessesAssocs.Include(r => r.AccountNavigation)
                .Where(r => tenancyIdsByTenant.Contains(r.IdProcess) && tenancyIdsByAddress.Contains(r.IdProcess)).ToList();

            var result = new List<KumiAccountsTenancyProcessesAssoc>();
            if (accounts.Any())
            {
                foreach (var account in accounts.GroupBy(r => r.IdProcess).OrderByDescending(r => r.Key).First())
                {
                    result.Add(new KumiAccountsTenancyProcessesAssoc
                    {
                        IdProcess = tenancyProcess.IdProcess,
                        IdAccount = account.IdAccount,
                        Fraction = account.Fraction,
                        AccountNavigation = new KumiAccount
                        {
                            Account = account.AccountNavigation.Account
                        }
                    });
                }
            }

            if (!result.Any())
            {
                string account = null; ;
                while (true)
                {
                    account = registryContext.GetNextKumiAccountNumber();
                    if (registryContext.KumiAccounts.Count(r => r.Account == account) == 0)
                        break;
                }
                result.Add(new KumiAccountsTenancyProcessesAssoc
                {
                    IdProcess = tenancyProcess.IdProcess,
                    Fraction = 1.0000m,
                    AccountNavigation = new KumiAccount
                    {
                        Account = account
                    }
                });
            }

            return result;
        }

        public void Edit(TenancyProcess tenancyProcess)
        {
            AddAndBindAccounts(tenancyProcess.AccountsTenancyProcessesAssoc, tenancyProcess.IdProcess);
            tenancyProcess.AccountsTenancyProcessesAssoc = null;
            registryContext.TenancyProcesses.Update(tenancyProcess);
            registryContext.SaveChanges();
        }

        public void AddAndBindAccounts(IList<KumiAccountsTenancyProcessesAssoc> accountsAssoc, int idProcess)
        {
            var assocsForUpdate = new List<KumiAccountsTenancyProcessesAssoc>();
            foreach (var assoc in accountsAssoc)
            {
                var account = assoc.AccountNavigation.Account;
                assoc.AccountNavigation = null;
                if (assoc.IdAccount != 0)
                {
                    assocsForUpdate.Add(assoc);
                    continue;
                }
                if (!string.IsNullOrEmpty(account))
                {
                    account = account.Trim();
                    var kumiAccount = registryContext.KumiAccounts.FirstOrDefault(r => r.Account == account);
                    if (kumiAccount == null)
                    {
                        kumiAccount = new KumiAccount
                        {
                            Account = account,
                            IdState = 1,
                            CurrentBalanceTenancy = 0,
                            CurrentBalancePenalty = 0,
                            CreateDate = DateTime.Now.Date,
                            RecalcMarker = 0
                        };
                        registryContext.KumiAccounts.Add(kumiAccount);
                        registryContext.SaveChanges();
                    }
                    assoc.IdAccount = kumiAccount.IdAccount;
                    assocsForUpdate.Add(assoc);
                }
            }

            var oldAccountTenancyAssocs = registryContext.KumiAccountsTenancyProcessesAssocs.Where(r => r.IdProcess == idProcess);
            foreach (var oldAssoc in oldAccountTenancyAssocs)
            {
                if (assocsForUpdate.Count(r => r.IdAssoc == oldAssoc.IdAssoc) > 0) continue;
                oldAssoc.Deleted = 1;
            }
            foreach (var newAssoc in assocsForUpdate)
            {
                if (newAssoc.IdAssoc != 0)
                {
                    var assocDb = registryContext.KumiAccountsTenancyProcessesAssocs.FirstOrDefault(r => r.IdAssoc == newAssoc.IdAssoc);
                    assocDb.Fraction = newAssoc.Fraction;
                    assocDb.IdProcess = idProcess;
                    assocDb.IdAccount = newAssoc.IdAccount;
                    registryContext.KumiAccountsTenancyProcessesAssocs.Update(assocDb);
                }
                else
                {
                    newAssoc.IdProcess = idProcess;
                    registryContext.KumiAccountsTenancyProcessesAssocs.Add(newAssoc);
                }
            }
        }

        public void Delete(int idProcess)
        {
            var tenancyProcesses = registryContext.TenancyProcesses
                    .FirstOrDefault(op => op.IdProcess == idProcess);
            if (tenancyProcesses != null)
            {
                tenancyProcesses.Deleted = 1;
                registryContext.SaveChanges();
            }
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
                if(filterOptions.RegistrationDateSign == "≥")
                    query = query.Where(p => p.RegistrationDate >= filterOptions.RegistrationDate);
                if(filterOptions.RegistrationDateSign == "≤")
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
                                 string.Concat(tpRow.Surname.Trim(), " ", tpRow.Name.Trim(), " ", tpRow.Patronymic == null ? "": tpRow.Patronymic.Trim()).ToLowerInvariant().Contains(tenantSnp)) ||
                             (tenancyParticipantSnp != null &&
                                 string.Concat(tpRow.Surname.Trim(), " ", tpRow.Name.Trim(), " ", tpRow.Patronymic == null ? "" : tpRow.Patronymic.Trim()).ToLowerInvariant().Contains(tenancyParticipantSnp)))
                         select tRow).Distinct();
            }
            if (filterOptions.TenantBirthDate.HasValue)
            {
                query = query.Where(p => p.TenancyPersons.Any(tr => tr.DateOfBirth == filterOptions.TenantBirthDate && tr.IdKinship==1));
            }
            if (filterOptions.TenancyParticipantBirthDate.HasValue)
            {
                query = query.Where(p => p.TenancyPersons.Any(tr => tr.DateOfBirth == filterOptions.TenancyParticipantBirthDate && tr.IdKinship!=1));
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
                } else
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

        public IQueryable<TenancyProcess> GetTenancyProcessesForMassReports(List<int> ids)
        {
            return registryContext.TenancyProcesses
                .Where(tp => ids.Contains(tp.IdProcess));
        }

        public TenancyProcessesVM GetTenancyProcessesViewModelForMassReports(List<int> ids, PageOptions pageOptions)
        {
            var viewModel = InitializeViewModel(null, pageOptions, null);
            var tenancyProcesses = GetTenancyProcessesForMassReports(ids);
            viewModel.PageOptions.TotalRows = tenancyProcesses.Count();
            var count = tenancyProcesses.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.TenancyProcesses = GetQueryPage(tenancyProcesses, viewModel.PageOptions).ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.TenancyProcesses);
            return viewModel;
        }

        public bool RegNumExist(string regNum, int idProcess)
        {
            var curRegNum = registryContext.TenancyProcesses
                .SingleOrDefault(tp => tp.IdProcess == idProcess)
                ?.RegistrationNum;
            if (curRegNum == regNum)
                return false;
            return registryContext.TenancyProcesses
                .Select(tp => tp.RegistrationNum)
                .ToList()
                .Any(num => num != null && num.Equals(regNum));
        }

        public IEnumerable<TenancyReasonType> TenancyReasonTypes
        {
            get => registryContext.TenancyReasonTypes.AsNoTracking();
        }

        public IEnumerable<Kinship> Kinships
        {
            get => registryContext.Kinships.AsNoTracking();
        }

        public IEnumerable<Executor> ActiveExecutors
        {
            get => registryContext.Executors.Where(e => !e.IsInactive).AsNoTracking();
        }

        public IEnumerable<KladrStreet> Streets
        {
            get => registryContext.KladrStreets.AsNoTracking();
        }

        public IEnumerable<TenancyProlongRentReason> TenancyProlongRentReasons {
            get => registryContext.TenancyProlongRentReasons.AsNoTracking();
        }

        public IEnumerable<DistrictCommittee> DistrictCommittees
        {
            get => registryContext.DistrictCommittees.AsNoTracking();
        }

        public IEnumerable<DistrictCommitteesPreContractPreamble> DistrictCommitteesPreContractPreambles
        {
            get => registryContext.DistrictCommitteesPreContractPreambles.AsNoTracking();
        }

        public IEnumerable<Preparer> Preparers
        {
            get => registryContext.Preparers.AsNoTracking();
        }

        public DateTime? AreaAvgCostActualDate
        {
            get => registryContext.TotalAreaAvgCosts.FirstOrDefault()?.Date;
        }
        public IEnumerable<SelectableSigner> BksSigners
        {
            get => registryContext.SelectableSigners.Where(s => s.IdSignerGroup == 1).AsNoTracking();
        }    }
}