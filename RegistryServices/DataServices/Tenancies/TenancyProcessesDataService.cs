using RegistryDb.Models;
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
using RegistryServices.DataFilterServices;
using RegistryServices.DataServices.Tenancies;

namespace RegistryWeb.DataServices
{
    public class TenancyProcessesDataService : ListDataService<TenancyProcessesVM, TenancyProcessesFilter>
    {
        private readonly SecurityService securityService;
        private readonly TenancyPaymentsDataService tenancyPayments;
        private readonly IConfiguration config;
        private readonly IFilterService<TenancyProcess, TenancyProcessesFilter> filterService;

        public TenancyProcessesDataService(RegistryContext registryContext, SecurityService securityService,
            AddressesDataService addressesDataService, TenancyPaymentsDataService tenancyPayments, IConfiguration config,
            FilterServiceFactory<IFilterService<TenancyProcess, TenancyProcessesFilter>> filterServiceFactory) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
            this.tenancyPayments = tenancyPayments;
            this.config = config;
            filterService = filterServiceFactory.CreateInstance();
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
            var query = filterService.GetQueryFilter(tenancyProcesses, viewModel.FilterOptions);
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            query = filterService.GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            filteredIds = query.Select(p => p.IdProcess).ToList();

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = filterService.GetQueryPage(query, viewModel.PageOptions);
            viewModel.TenancyProcesses = query.ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.TenancyProcesses);
            viewModel.AreaAvgCostActualDate = registryContext.TotalAreaAvgCosts.FirstOrDefault()?.Date;
            return viewModel;
        }

        public IQueryable<TenancyProcess> GetTenancyProcesses(TenancyProcessesFilter filterOptions)
        {
            var tenancyProcesses = GetQuery();
            var query = filterService.GetQueryFilter(tenancyProcesses, filterOptions);
            query = query.Include(r => r.AccountsTenancyProcessesAssoc).Include(r => r.TenancyPersons);
            return query;
        }

        private IQueryable<TenancyProcess> GetQuery()
        {
            return registryContext.TenancyProcesses.Include(r => r.AccountsTenancyProcessesAssoc);
        }

        public Dictionary<int, List<TenancyRentObject>> GetRentObjects(IEnumerable<TenancyProcess> tenancyProcesses)
        {
            var objects = tenancyPayments.GetTenancyPaymentRentObjectInfo(tenancyProcesses, false).ToList();

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

            // Если догово не безвозмездного пользования, то сделать связку с лицевым счетом
            if (tenancyProcess.IdRentType != 5)
            {
                if (!assocs.Any(r => r.IdAccount != 0 || !string.IsNullOrEmpty(r.AccountNavigation.Account)))
                {
                    assocs = FindAccountFromPrevTenancyOrCreateNew(assocs, tenancyProcess);
                }
                AddAndBindAccounts(assocs, tenancyProcess.IdProcess);
                registryContext.SaveChanges();
            }
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
            if (accountsAssoc != null)
            {
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
            viewModel.TenancyProcesses = filterService.GetQueryPage(tenancyProcesses, viewModel.PageOptions).ToList();
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
        }
    }
}