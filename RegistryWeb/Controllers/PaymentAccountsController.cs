﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.Models;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PaymentAccountsController : ListController<PaymentAccountsDataService, PaymentsFilter>
    {
        public PaymentAccountsController(PaymentAccountsDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
            nameFilteredIdsDict = "filteredAccountsIdsDict";
            nameIds = "idAccounts";
            nameMultimaster = "AccountsReports";
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

        public IActionResult Details(int idAccount, string returnUrl)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            ViewBag.SecurityService = securityService;
            ViewBag.SignersReports = dataService.Signers.Where(r => r.IdSignerGroup == 2).ToList();
            ViewBag.IdAccount = idAccount;
            ViewBag.ReturnUrl = returnUrl;
            var paymentsVM = dataService.GetPaymentsHistory(idAccount);
            if (!paymentsVM.Payments.Any())
                return Error("Ошибка формирования списка платежей по лицевому счету");
            return View(paymentsVM);
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

            ViewBag.ErrorPayments = dataService.GetPaymentsForMassReports(errorIds).ToList();

            var ids = GetSessionIds();
            var viewModel = dataService.GetPaymentsViewModelForMassReports(ids, pageOptions);
            ViewBag.Count = viewModel.Payments.Count();
            ViewBag.SignersReports = dataService.Signers.Where(r => r.IdSignerGroup == 2).ToList();
            ViewBag.CurrentExecutor = dataService.CurrentExecutor?.ExecutorName;
            ViewBag.CanEdit = securityService.HasPrivilege(Privileges.ClaimsWrite);
            return View("AccountReports", viewModel);
        }

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

                if (hasOpenedClaims)
                {
                    errorIds.Add(payment.IdAccount);
                }
                else
                {
                    processingIds.Add(payment.IdAccount);
                }
            }

            dataService.CreateClaimMass(processingIds, atDate);

            TempData["ErrorAccountsIds"] = JsonConvert.SerializeObject(errorIds);
            TempData["ErrorReason"] = "по указанным лицевым счетам уже имеются незавершенные исковые работы";
            return RedirectToAction("AccountsReports");
        }
    }
}