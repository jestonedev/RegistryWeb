using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Extensions;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.Enums;
using System;
using Newtonsoft.Json;

namespace RegistryWeb.Controllers
{
    public class KumiAccountsController :  ListController<KumiAccountsDataService, KumiAccountsFilter>
    {
        private TenancyProcessesDataService tenancyProcessesDataService { get; }

        public KumiAccountsController(KumiAccountsDataService dataService, TenancyProcessesDataService tenancyProcessesDataService, SecurityService securityService)
            :base(dataService, securityService)
        {
            nameFilteredIdsDict = "filteredKumiAccountsIdsDict";
            nameIds = "idKumiAccounts";
            nameMultimaster = "KumiAccountsReports";
            this.tenancyProcessesDataService = tenancyProcessesDataService;
        }

        public IActionResult Index(KumiAccountsVM viewModel, bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.AccountsRead))
                return View("NotAccess");
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<KumiAccountsFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
            ViewBag.SecurityService = securityService;

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions, out List<int> filteredTenancyProcessesIds);

            AddSearchIdsToSession(vm.FilterOptions, filteredTenancyProcessesIds);

            ViewBag.LastChargeDate = (DateTime?)null;

            return View(vm);
        }

        public IActionResult KumiAccountsReports(PageOptions pageOptions)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            var errorIds = new List<int>();
            if (TempData.ContainsKey("ErrorAccountsIds"))
            {
                try
                {
                    errorIds = JsonConvert.DeserializeObject<List<int>>(TempData["ErrorAccountsIds"].ToString());
                }
                catch
                {
                }
                TempData.Remove("ErrorAccountsIds");
            }
            if (TempData.ContainsKey("ErrorReason"))
            {
                ViewBag.ErrorReason = TempData["ErrorReason"];
                TempData.Remove("ErrorReason");
            }
            else
            {
                ViewBag.ErrorReason = "неизвестно";
            }

            ViewBag.ErrorAccounts = dataService.GetAccountsForMassReports(errorIds).ToList();

            var ids = GetSessionIds();
            var viewModel = dataService.GetAccountsViewModelForMassReports(ids, pageOptions);
            ViewBag.Count = viewModel.Accounts.Count();
            ViewBag.CanEdit = securityService.HasPrivilege(Privileges.AccountsWrite);
            ViewBag.Emails = dataService.GetTenantsEmails(viewModel.Accounts.ToList());
            ViewBag.LastChargeDate = (DateTime?)null;
            return View("AccountReports", viewModel);
        }

        public IActionResult DetailsByAddress(int idAccount, string returnUrl)
        {
            if (!securityService.HasPrivilege(Privileges.AccountsRead))
                return View("NotAccess");
            var accounts = dataService.GetKumiAccountsOnSameAddress(idAccount);
            ViewBag.Address = dataService.GetAccountAddress(idAccount);
            ViewBag.IdAccountCurrent = idAccount;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.States = dataService.States;
            ViewBag.SecurityService = securityService;
            var tenancyInfo = dataService.GetTenancyInfo(accounts);
            ViewBag.TenancyInfo = tenancyInfo;
            return View("DetailsByAddress", accounts);
        }

        private IActionResult GetView(int? idAccount, string returnUrl, ActionTypeEnum action, Privileges privilege)
        {
            if (!securityService.HasPrivilege(privilege))
                return View("NotAccess");
            KumiAccount account = new KumiAccount();
            if (action != ActionTypeEnum.Create)
            {
                if (!idAccount.HasValue)
                    return NotFound();
                account = dataService.GetKumiAccount(idAccount.Value, true);
                if (account == null)
                    return NotFound();
            } else
            {
                while (true)
                {
                    account.Account = dataService.GetNextKumiAccountNumber();
                    if (!dataService.AccountExists(account.Account))
                        break;
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Action = action;
            ViewBag.SecurityService = securityService;
            ViewBag.States = dataService.States;
            ViewBag.Streets = dataService.Streets;
            ViewBag.Regions = dataService.Regions;
            var tenancyInfo = dataService.GetTenancyInfo(new List<KumiAccount> { account });
            if (tenancyInfo.ContainsKey(account.IdAccount))
                ViewBag.TenancyInfo = tenancyInfo[account.IdAccount];
            else
                ViewBag.TenancyInfo = new List<KumiAccountTenancyInfoVM>();
            return View("Account", account);
        }

        [HttpGet]
        public IActionResult Create(string returnUrl)
        {
            return GetView(null, returnUrl, ActionTypeEnum.Create, Privileges.AccountsReadWrite);
        }

        [HttpGet]
        public IActionResult Details(int? idAccount, string returnUrl)
        {
            return GetView(idAccount, returnUrl, ActionTypeEnum.Details, Privileges.AccountsRead);
        }

        [HttpGet]
        public IActionResult Edit(int? idAccount, string returnUrl)
        {
            return GetView(idAccount, returnUrl, ActionTypeEnum.Edit, Privileges.AccountsReadWrite);
        }

        [HttpGet]
        public IActionResult Delete(int? idAccount, string returnUrl)
        {
            return GetView(idAccount, returnUrl, ActionTypeEnum.Delete, Privileges.AccountsReadWrite);
        }

        [HttpPost]
        public IActionResult Create(KumiAccount kumiAccount)
        {
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite);
            if (!canEdit)
                return View("NotAccess");
            if (kumiAccount == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create(kumiAccount);
                return RedirectToAction("Details", new { kumiAccount.IdAccount });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(KumiAccount kumiAccount)
        {
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite);
            if (!canEdit)
                return View("NotAccess");
            if (kumiAccount == null)
                return NotFound();
            dataService.Delete(kumiAccount.IdAccount);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(KumiAccount kumiAccount)
        {
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite);
            if (!canEdit)
                return View("NotAccess");
            if (kumiAccount == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Edit(kumiAccount);
                return RedirectToAction("Details", new { kumiAccount.IdAccount });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AccountExist(string account, int idAccount)
        {
            if (account == null)
                return Json(false);
            var isExist = dataService.AccountExists(account, idAccount);
            return Json(isExist);
        }

        [HttpPost]
        public IActionResult GetTenancyInfo(TenancyProcessesFilter filterOptions)
        {
            var tenancies = tenancyProcessesDataService.GetTenancyProcesses(filterOptions);
            var count = tenancies.Count();
            var tenanciesLimit = tenancies.Take(5).ToList();
            var rentObjects = tenancyProcessesDataService.GetRentObjects(tenanciesLimit);
            var accounts = dataService.GetAccountsForTenancies(tenanciesLimit);
            return Json(new {
                Count = count,
                Tenancies = tenanciesLimit.Select(r => new {
                    r.IdProcess,
                    r.RegistrationDate,
                    r.RegistrationNum,
                    Tenant = r.TenancyPersons.Where(p => p.IdKinship == 1 && p.ExcludeDate == null)
                        .Select(p => p.Surname + " " + p.Name + (p.Patronymic!=null ? " " + p.Patronymic : "")).FirstOrDefault(),
                    AccountsInfo = accounts.Where(a => a.AccountsTenancyProcessesAssoc.Any(assoc => assoc.IdProcess == r.IdProcess))
                    .Select(a => new {
                        a.IdAccount,
                        a.Account
                    })
                }),
                RentObjects = rentObjects
            });
        }

        [HttpPost]
        public IActionResult RecalcAccounts(List<int> idAccounts, KumiAccountRecalcTypeEnum recalcType, int? recalcStartYear, int? recalcStartMonth,
            bool saveCurrentPeriodCharge)
        {
            try
            {
                DateTime? recalcStartDate = null;
                if (recalcStartYear != null && recalcStartMonth != null)
                    recalcStartDate = new DateTime(recalcStartYear.Value, recalcStartMonth.Value, 1);
                dataService.RecalculateAccounts(idAccounts, recalcType, recalcStartDate, saveCurrentPeriodCharge);

                return Json(new
                {
                    State = "Success"
                });
            }
            catch(Exception e)
            {
                return Json(new
                {
                    State = "Error",
                    Error = e.Message
                });
            }
        }

        public IActionResult AddChargeCorrection(int idAccount, decimal tenancyValue, decimal penaltyValue, 
            decimal dgiValue, decimal pkkValue, decimal padunValue,
            decimal paymentTenancyValue, decimal paymentPenaltyValue, 
            decimal paymentDgiValue, decimal paymentPkkValue, decimal paymentPadunValue,
            DateTime atDate, string description, int? idAccountMirror)
        {
            try
            {
                dataService.AddChargeCorrection(idAccount, tenancyValue, penaltyValue, dgiValue, pkkValue, padunValue,
                    paymentTenancyValue, paymentPenaltyValue, paymentDgiValue, paymentPkkValue, paymentPadunValue,
                    atDate, description, idAccountMirror);
                return Json(new
                {
                    State = "Success"
                });
            } catch(Exception e)
            {
                return Json(new
                {
                    State = "Error",
                    Error = e.Message
                });
            }
        }

        public IActionResult ChargeCorrectionsList(int idAccount)
        {
            return View(dataService.GetAccountCorrectionsVm(idAccount));
        }

        public IActionResult DeleteCorrection(int idCorrection)
        {
            try
            {
                var idAccount = dataService.GetIdAccountByCorrection(idCorrection);
                dataService.DeleteChargeCorrection(idCorrection);
                dataService.RecalculateAccounts(new List<int> { idAccount }, 0, null, false);
                return Json(new
                {
                    State = "Success"
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    State = "Error",
                    Error = e.Message
                });
            }
        }

        [HttpPost]
        public IActionResult CalcForecastPeriod(int idAccount, DateTime calcToDate) {
            var charge = dataService.CalcForecastChargeInfo(idAccount, calcToDate);
            return Json(charge);
        }

        public IActionResult SaveDescriptionForAddress(int idAccount, string description) {
            try
            {
                dataService.SaveDescriptionForAddress(idAccount, description);
                return Json(new
                {
                    State = "Success"
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    State = "Error",
                    Error = e.Message
                });
            }
        }

        public IActionResult CreateClaimMass(DateTime atDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Error("У вас нет прав на выполнение данной операции");

            var ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            var accountsVM = dataService.GetAccountsViewModelForMassReports(ids, new PageOptions { SizePage = int.MaxValue });
            var processingIds = new List<int>();
            var errorIds = new List<int>();
            foreach (var account in accountsVM.Accounts)
            {
                var hasOpenedClaims = false;
                if (accountsVM.ClaimsInfo.ContainsKey(account.IdAccount))
                {
                    var lastClaimInfo = accountsVM.ClaimsInfo[account.IdAccount].First();
                    if (accountsVM.ClaimsInfo[account.IdAccount].Any(r => r.IdClaimCurrentState != 6 && !r.EndedForFilter))
                    {
                        lastClaimInfo = accountsVM.ClaimsInfo[account.IdAccount].FirstOrDefault(r => r.IdClaimCurrentState != 6 && !r.EndedForFilter);
                    }
                    hasOpenedClaims = lastClaimInfo.IdClaimCurrentState != 6 && !lastClaimInfo.EndedForFilter;
                }

                if (hasOpenedClaims)
                {
                    errorIds.Add(account.IdAccount);
                }
                else
                {
                    processingIds.Add(account.IdAccount);
                }
            }

            dataService.CreateClaimMass(processingIds, atDate);

            TempData["ErrorAccountsIds"] = JsonConvert.SerializeObject(errorIds);
            TempData["ErrorReason"] = "по указанным лицевым счетам уже имеются незавершенные исковые работы";
            return RedirectToAction("KumiAccountsReports");
        }

        // Сформировать акт по лицевому счету на дату
        public IActionResult GetActCharge(int idAccount, DateTime atDate, string returnUrl)
        {
            var actChargeVMs = dataService.GetActChargeVMs(idAccount, atDate);
            ViewBag.Account = dataService.GetKumiAccount(idAccount);
            ViewBag.AtDate = atDate;
            ViewBag.ReturnUrl = returnUrl;
            return View("ActCharge", actChargeVMs);
        }

        public IActionResult OpenPenaltyCalculator(int idAccount, DateTime? startDate, DateTime? endDate)
        {
            return RedirectPermanent("/wwwroot/peni_calc/mcalc.html#"+dataService.GetUrlForPenaltyCalculator(idAccount, startDate, endDate));
        }
    }
}