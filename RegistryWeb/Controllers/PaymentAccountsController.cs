using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.Payments;
using RegistryServices.DataServices.BksAccounts;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PaymentAccountsController : ListController<PaymentAccountsDataService, PaymentsFilter>
    {
        private readonly PaymentAccountsCommonService commonService;
        private readonly PaymentAccountsTenanciesService tenanciesService;
        private readonly PaymentAccountsClaimsService claimsService;

        public PaymentAccountsController(PaymentAccountsDataService dataService, 
            PaymentAccountsCommonService commonService,
            PaymentAccountsTenanciesService tenanciesService,
            PaymentAccountsClaimsService claimsService,
            SecurityService securityService)
            : base(dataService, securityService)
        {
            nameFilteredIdsDict = "filteredAccountsIdsDict";
            nameIds = "idAccounts";
            nameMultimaster = "AccountsReports";
            this.commonService = commonService;
            this.tenanciesService = tenanciesService;
            this.claimsService = claimsService;
        }

        public IActionResult Index(PaymentsVM viewModel, bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<PaymentsFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
            ViewBag.SecurityService = securityService;
            ViewBag.SignersReports = dataService.Signers.Where(r => r.IdSignerGroup == 2).ToList();

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions, out List<int> filteredTenancyProcessesIds);

            AddSearchIdsToSession(vm.FilterOptions, filteredTenancyProcessesIds);

            return View(vm);
        }

        public IActionResult PaymentAccountsTable(int idAccount, string returnUrl)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            if (idAccount == 0)
                return NotFound();
            ViewBag.IdAccount = idAccount;
            ViewBag.ReturnUrl = returnUrl;
            var vm = dataService.GetPaymentHistoryTable(securityService.User, idAccount);
            if (!vm.Payments.Any())
                return Error("Ошибка формирования списка платежей по лицевому счету");
            return View("PaymentAccountsTable", vm);
        }

        public IActionResult PaymentAccountsRentObjectTable(int idAccount, string returnUrl)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            if (idAccount == 0)
                return NotFound();
            ViewBag.IdAccount = idAccount;
            ViewBag.ReturnUrl = returnUrl;
            var vm = dataService.GetPaymentHistoryRentObjectTable(securityService.User, idAccount);
            if (!vm.Payments.Any())
                return Error("Ошибка формирования списка платежей по жилому помещению");
            ViewBag.PaymentsByAddress = true;
            return View("PaymentAccountsTable", vm);
        }

        [HttpPost]
        public JsonResult SavePaymentAccountTableJson(PaymentAccountTableJson vm)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Json(false);
            var result =
                dataService.SavePaymentAccountTableJson(securityService.User, vm);
            return Json(result);
        }

        [HttpGet]
        public JsonResult GetPrimisesInfo(int? idPremise)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead) || (idPremise == null) || (idPremise == 0))
                return Json(false);
            var premise = dataService.GetPremiseJson(idPremise.Value);
            if (premise == null)
                return Json(false);
            return Json(premise, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        [HttpGet]
        public IActionResult GetRestrictionsInfo(int idPremise)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Content("");
            return ViewComponent("RestrictionsComponent", new { id = idPremise, type = AddressTypes.Premise, isTable = true });
        }

        [HttpPost]
        public IActionResult GetTenanciesInfo(int id, AddressTypes type, string returnUrl)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Content("");
            var address = new Address() { Id = id.ToString(), AddressType = type };
            return ViewComponent("TenancyProcessesComponent", new { address, returnUrl });
        }

        public IActionResult AccountsReports(PageOptions pageOptions)
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

            ViewBag.ErrorPayments = commonService.GetPaymentsForMassReports(errorIds).ToList();

            var ids = GetSessionIds();
            var viewModel = dataService.GetPaymentsViewModelForMassReports(ids, pageOptions);
            ViewBag.Count = viewModel.Payments.Count();
            ViewBag.SignersReports = dataService.Signers.Where(r => r.IdSignerGroup == 2).ToList();
            ViewBag.CurrentExecutor = securityService.CurrentExecutor?.ExecutorName;
            ViewBag.CanEdit = securityService.HasPrivilege(Privileges.ClaimsWrite);
            ViewBag.Emails = tenanciesService.GetTenantsEmails(viewModel.Payments.Select(r => r.IdAccount).ToList());
            ViewBag.EmailsModified = tenanciesService.GetTenantsEmailsModified(viewModel.Payments.Select(r => r.IdAccount).ToList());
            return View("AccountReports", viewModel);
        }

        /*public IActionResult CreateClaimMassCustom()
        {
            dataService.CreateClaimMassCustom();
            return Content("Complete");
        }*/

        public IActionResult CreateClaimMass(DateTime atDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Error("У вас нет прав на выполнение данной операции");

            var ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            var paymentsVM = dataService.GetPaymentsViewModelForMassReports(ids, new PageOptions { SizePage = int.MaxValue });
            var processingIds = new List<int>();
            var errorIds = new List<int>();
            var accountsIdsAssoc = commonService.GetAccountIdsAssocs(paymentsVM.Payments);
            foreach (var payment in paymentsVM.Payments)
            {
                var hasOpenedClaims = false;
                if (paymentsVM.ClaimsByAddresses.ContainsKey(payment.IdAccount))
                {
                    var lastClaimInfo = paymentsVM.ClaimsByAddresses[payment.IdAccount].First();
                    if (paymentsVM.ClaimsByAddresses[payment.IdAccount].Any(r => r.IdClaimCurrentState != 6 && !r.EndedForFilter))
                    {
                        lastClaimInfo = paymentsVM.ClaimsByAddresses[payment.IdAccount].FirstOrDefault(r => r.IdClaimCurrentState != 6 && !r.EndedForFilter);
                    }
                    hasOpenedClaims = lastClaimInfo.IdClaimCurrentState != 6 && !lastClaimInfo.EndedForFilter;
                }
                // Если в мастере есть лицевые счета на одно и то же помещение, то необходимо создать только одну искову работу:
                // на последний по идентификатор лицевой счет
                var accountsAssoc = accountsIdsAssoc.Where(r => r.IdAccountFiltered == payment.IdAccount);
                var paymentsDuplicates = paymentsVM.Payments.Where(r => accountsAssoc.Select(a => a.IdAccountActual).Contains(r.IdAccount));
                if (paymentsDuplicates.Max(r => r.IdAccount) > payment.IdAccount)
                {
                    continue;
                }

                if (hasOpenedClaims)
                {
                    errorIds.Add(payment.IdAccount);
                }
                else
                {
                    processingIds.Add(payment.IdAccount);
                }
            }

            claimsService.CreateClaimMass(processingIds, atDate);

            TempData["ErrorAccountsIds"] = JsonConvert.SerializeObject(errorIds);
            TempData["ErrorReason"] = "по указанным лицевым счетам уже имеются незавершенные исковые работы";
            return RedirectToAction("AccountsReports");
        }
        public JsonResult AddComment(int idAccount,  string textComment, string path)
        {
            return Json(dataService.AddCommentsForPaymentAccount(idAccount, textComment, path));
        }

    }
}