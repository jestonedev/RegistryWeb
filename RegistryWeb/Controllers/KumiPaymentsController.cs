﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader;
using RegistryPaymentsLoader.TffFileLoaders;
using RegistryPaymentsLoader.TffStrings;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RegistryWeb.Extensions;
using RegistryWeb.ViewOptions;
using RegistryWeb.Enums;
using RegistryDb.Models.Entities.Claims;
using RegistryServices.Enums;

namespace RegistryWeb.Controllers
{
    public class KumiPaymentsController : ListController<KumiPaymentsDataService, KumiPaymentsFilter>
    {
        private readonly KumiAccountsDataService kumiAccountsDataService;
        private readonly ClaimsDataService claimsDataService;
        private readonly TenancyProcessesDataService tenancyProcessesDataService;
        private readonly ZipArchiveDataService zipArchiveDataService;

        public KumiPaymentsController(KumiPaymentsDataService dataService, 
            KumiAccountsDataService kumiAccountsDataService,
            ClaimsDataService claimsDataService,
            TenancyProcessesDataService tenancyProcessesDataService, 
            SecurityService securityService, ZipArchiveDataService zipArchiveDataService)
            : base(dataService, securityService)
        {
            this.kumiAccountsDataService = kumiAccountsDataService;
            this.claimsDataService = claimsDataService;
            this.tenancyProcessesDataService = tenancyProcessesDataService;
            this.zipArchiveDataService = zipArchiveDataService;
        }

        public IActionResult Index(KumiPaymentsVM viewModel, bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.AccountsRead))
                return View("NotAccess");
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<KumiPaymentsFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
            ViewBag.SecurityService = securityService;
            ViewBag.PaymentUfSigners = dataService.PaymentUfSigners.Select(r => new { r.IdRecord, Snp = (r.Surname + " " + r.Name + " " + r.Patronymic).Trim() });
            ViewBag.Regions = dataService.Regions;
            ViewBag.Streets = dataService.Streets;
            ViewBag.AccountStates = dataService.AccountStates;
            ViewBag.ClaimStateTypes = dataService.ClaimStateTypes;

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions);

            return View(vm);
        }

        private IActionResult GetView(int? idPayment, string returnUrl, ActionTypeEnum action, Privileges privilege)
        {
            if (!securityService.HasPrivilege(privilege))
                return View("NotAccess");
            KumiPayment payment = new KumiPayment {
                IdSource = 1
            };
            if (action != ActionTypeEnum.Create)
            {
                if (!idPayment.HasValue)
                    return NotFound();
                payment = dataService.GetKumiPayment(idPayment.Value);
                if (payment == null)
                    return NotFound();
            }
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Action = action;
            ViewBag.SecurityService = securityService;
            ViewBag.PaymentGroups = dataService.PaymentGroups;
            ViewBag.PaymentInfoSources = dataService.PaymentInfoSources.Select(r => new { r.IdSource, Name = "("+r.Code+") " + r.Name });
            ViewBag.PaymentDocCodes = dataService.PaymentDocCodes.Select(r => new { r.IdPaymentDocCode, Name = "(" + r.Code + ") " + r.Name });
            ViewBag.PaymentKinds = dataService.PaymentKinds.Select(r => new { r.IdPaymentKind, Name = "(" + r.Code + ") " + r.Name });
            ViewBag.OperationTypes = dataService.OperationTypes.Select(r => new { r.IdOperationType, Name = "(" + r.Code + ") " + r.Name });
            ViewBag.KbkTypes = dataService.KbkTypes.Select(r => new { r.IdKbkType, Name = "(" + r.Code + ") " + r.Name });
            ViewBag.PaymentReasons = dataService.PaymentReasons.Select(r => new { r.IdPaymentReason, Name = "(" + r.Code + ") " + r.Name });
            ViewBag.PayerStatuses = dataService.PayerStatuses.Select(r => new { r.IdPayerStatus, Name = "(" + r.Code + ") " + r.Name });
            ViewBag.PaymentUfSigners = dataService.PaymentUfSigners.Select(r => new { r.IdRecord, Snp = (r.Surname + " " + r.Name + " " + r.Patronymic).Trim() });
            ViewBag.Regions = dataService.Regions;
            ViewBag.Streets = dataService.Streets;
            ViewBag.AccountStates = dataService.AccountStates;
            ViewBag.ClaimStateTypes = dataService.ClaimStateTypes;

            return View("Payment", payment);
        }

        [HttpGet]
        public IActionResult Create(string returnUrl)
        {
            return GetView(null, returnUrl, ActionTypeEnum.Create, Privileges.AccountsReadWrite);
        }

        [HttpGet]
        public IActionResult Details(int? idPayment, string returnUrl)
        {
            return GetView(idPayment, returnUrl, ActionTypeEnum.Details, Privileges.AccountsRead);
        }

        [HttpGet]
        public IActionResult Edit(int? idPayment, string returnUrl)
        {
            return GetView(idPayment, returnUrl, ActionTypeEnum.Edit, Privileges.AccountsReadWrite);
        }

        [HttpGet]
        public IActionResult Delete(int? idPayment, string returnUrl)
        {
            return GetView(idPayment, returnUrl, ActionTypeEnum.Delete, Privileges.AccountsReadWrite);
        }

        [HttpPost]
        public IActionResult Create(KumiPayment kumiPayment)
        {
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite) && kumiPayment.IdSource == 1;
            if (!canEdit)
                return View("NotAccess");
            if (kumiPayment == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create(kumiPayment);
                return RedirectToAction("Details", new { kumiPayment.IdPayment });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(KumiPayment kumiPayment)
        {
            var dbPayment = dataService.GetKumiPayment(kumiPayment.IdPayment);
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite) && dbPayment.IdSource == 1;
            if (!canEdit)
                return View("NotAccess");
            if (kumiPayment == null)
                return NotFound();

            if (dbPayment.PaymentClaims.Any() || dbPayment.PaymentCharges.Any())
                return Error("Нельзя удалить распределенный платеж");

            dataService.Delete(kumiPayment.IdPayment);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(KumiPayment kumiPayment)
        {
            var dbPayment = dataService.GetKumiPayment(kumiPayment.IdPayment);
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite) && dbPayment.IdSource == 1;
            if (!canEdit)
                return View("NotAccess");
            if (kumiPayment == null)
                return NotFound();

            if (dbPayment.PaymentClaims.Any() || dbPayment.PaymentCharges.Any())
                return Error("Нельзя изменить распределенный платеж");

            if (ModelState.IsValid)
            {
                dataService.Edit(kumiPayment);
                return RedirectToAction("Details", new { kumiPayment.IdPayment });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddPaymentUf(int? IdPayment)
        {
            if (!securityService.HasPrivilege(Privileges.AccountsWrite))
                return Json(-2);
            if (IdPayment == null) return Json(-3);

            var dbPayment = dataService.GetKumiPayment(IdPayment.Value);

            if (dbPayment == null) return Json(-4);

            var numUf = dataService.GetKumiPaymentUfsLastNumber();

            var paymentUf = new KumiPaymentUf {
                NumUf = numUf == null ? "" : (numUf + 1).ToString(),
                DateUf = DateTime.Now.Date,
                Sum = dbPayment.Sum,
                Purpose = dbPayment.Purpose,
                Kbk = dbPayment.Kbk,
                IdKbkType = dbPayment.IdKbkType,
                TargetCode = dbPayment.TargetCode,
                Okato = dbPayment.Okato,
                RecipientInn = dbPayment.RecipientInn,
                RecipientKpp = dbPayment.RecipientKpp,
                RecipientName = dbPayment.RecipientName,
                RecipientAccount = dbPayment.RecipientAccount
            };
            ViewBag.CanEditUfs = true;

            return PartialView("PaymentUf", paymentUf);
        }

        public IActionResult DownloadPaymentUf(int idPaymentUf, int idSigner, DateTime? signDate)
        {
            var paymentUf = dataService.GetKumiPaymentUf(idPaymentUf);
            var paymentSettings = dataService.GetKumiPaymentSettings();
            var signer = dataService.GetSigner(idSigner);
            var file = dataService.GetPaymentUfsFile(new List<KumiPaymentUf> { paymentUf }, paymentSettings, signer, signDate ?? DateTime.Now.Date);
            return File(file, "application/octet-stream", string.Format("{0}-{1} (уведомление).uf", paymentSettings.CodeUbp, paymentUf.NumUf));
        }

        public IActionResult DownloadPaymentUfs(int idSigner, DateTime? signDate, DateTime? dateUf)
        {
            var paymentUfs = dataService.GetKumiPaymentUfs(dateUf ?? DateTime.Now.Date);
            if (paymentUfs.Count == 0) return Error(string.Format("Уведомления на дату {0} не найдены", (dateUf ?? DateTime.Now.Date).ToString("dd.MM.yyyy")));
            var paymentSettings = dataService.GetKumiPaymentSettings();
            var signer = dataService.GetSigner(idSigner);
            var file = dataService.GetPaymentUfsFile(paymentUfs, paymentSettings, signer, signDate ?? DateTime.Now.Date);
            return File(file, "application/octet-stream", string.Format("{0}-{1} (уведомления).uf", paymentSettings.CodeUbp, paymentUfs.Count));
        }

        public IActionResult GetMemorialOrders(MemorialOrderFilter filterOptions)
        {
            var mo = dataService.GetMemorialOrders(filterOptions);
            var count = mo.Count();
            var moLimit = mo.Take(5).ToList();
            return Json(new
            {
                Count = count,
                MemorialOrders = moLimit.Select(r => new {
                    r.IdOrder,
                    r.NumDocument,
                    r.DateDocument,
                    r.SumZach,
                    r.Kbk,
                    r.Okato
                })
            });
        }

        public IActionResult GetDistributePaymentToObjects(DistributePaymentToObjectFilter filterOptions)
        {
            IQueryable<KumiAccount> accountsResult = null;
            IQueryable<Claim> claimsResult = null;
            if (!filterOptions.IsTenancyEmpty())
            {
                var tenancies = tenancyProcessesDataService.GetTenancyProcesses(filterOptions);
                accountsResult = tenancies.Where(r => r.IdAccount != null).Select(r => r.IdAccountNavigation);
            }

            if(!filterOptions.IsAccountEmpty())
            {
                var accounts = kumiAccountsDataService.GetKumiAccounts(new KumiAccountsFilter
                {
                    Account = filterOptions.Account,
                    AccountGisZkh = filterOptions.AccountGisZkh,
                    IdAccountState = filterOptions.IdAccountState
                });
                if (accountsResult == null)
                    accountsResult = accounts;
                else
                {
                    var ids = accounts.Select(r => r.IdAccount).ToList();
                    accountsResult = accountsResult.Where(r => ids.Contains(r.IdAccount));
                }
            }

            if (!filterOptions.IsClaimEmpty())
            {
                claimsResult = claimsDataService.GetClaimsForPaymentDistribute(new ClaimsFilter {
                    AtDate = filterOptions.ClaimAtDate,
                    CourtOrderNum = filterOptions.ClaimCourtOrderNum,
                    IdClaimState = filterOptions.ClaimIdStateType,
                    IsCurrentState = true
                });
            }


            if (filterOptions.DistributeTo == KumiPaymentDistributeToEnum.ToClaim)
            {
                if (accountsResult != null)
                {
                    var claims = claimsDataService.GetClaimsByAccountIdsForPaymentDistribute(accountsResult.Select(r => r.IdAccount).ToList());
                    if (claimsResult == null)
                        claimsResult = claims;
                    else
                    {
                        var ids = claims.Select(r => r.IdClaim).ToList();
                        claimsResult = claimsResult.Where(r => ids.Contains(r.IdClaim));
                    }
                }

                if (claimsResult == null)
                    claimsResult = new List<Claim>().AsQueryable();
                var count = claimsResult.Count();
                if (count > 3)
                    claimsResult = claimsResult.Take(3);
                return Json(new
                {
                    Count = count,
                    Claims = claimsResult.Select(r => new {
                        r.IdClaim,
                        r.IdAccountKumiNavigation.Account,
                        IdAccount = r.IdAccountKumi,
                        AccountState = r.IdAccountKumiNavigation.State.State,
                        IdAccountState = r.IdAccountKumiNavigation.IdState,
                        AmountTenancy = r.AmountTenancy + r.AmountDgi + r.AmountPadun + r.AmountPkk,
                        r.AmountPenalties,
                        r.AmountTenancyRecovered,
                        r.AmountPenaltiesRecovered,
                        r.StartDeptPeriod,
                        r.EndDeptPeriod
                    })
                });
            }
            else
            {
                if (accountsResult == null)
                    accountsResult = new List<KumiAccount>().AsQueryable();
                var count = accountsResult.Count();
                if (count > 3)
                    accountsResult = accountsResult.Take(3);
                return Json(new
                {
                    Count = count,
                    Accounts = accountsResult.Select(r => new {
                        r.IdAccount,
                        r.Account,
                        r.State.State,
                        r.State.IdState,
                        r.LastChargeDate,
                        r.CurrentBalanceTenancy,
                        r.CurrentBalancePenalty
                    })
                });
            }
        }

        [HttpPost]
        public IActionResult CreateByMemorialOrder(int idOrder, string returnUrl)
        {
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite);
            if (!canEdit)
                return View("NotAccess");
            try
            {
                var payment = dataService.CreateByMemorialOrder(idOrder);
                return Json(new
                {
                    State = "Success",
                    RedirectUrl = Url.Action("Details", new { payment.IdPayment, returnUrl })
                });
            } catch (Exception e)
            {
                return Json(new
                {
                    State = "Error",
                    Error = e.Message
                });
            }

        }

        public IActionResult ApplyMemorialOrder(int idPayment, int idOrder, string returnUrl)
        {
            try
            {
                dataService.ApplyMemorialOrderToPayment(idPayment, idOrder, out bool updatedExistsPayment);
                if (updatedExistsPayment)
                {
                    return Json(new
                    {
                        State = "Success",
                        RedirectUrl = Url.Action("Details", new { IdPayment = idPayment, returnUrl })
                    });
                } else
                    return Json(new
                    {
                        State = "Success",
                        RedirectUrl = "/KumiPayments/Index?FilterOptions.IdParentPayment=" + idPayment
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

        public IActionResult DistributePaymentToAccount(int idPayment, int idObject, KumiPaymentDistributeToEnum distributeTo,
            decimal tenancySum, decimal penaltySum)
        {
            try
            {
                var paymentDistributionInfo = dataService.DistributePaymentToAccount(idPayment, idObject, distributeTo, tenancySum, penaltySum);
                return Json(new
                {
                    State = "Success",
                    paymentDistributionInfo.Sum,
                    paymentDistributionInfo.DistrubutedToTenancySum,
                    paymentDistributionInfo.DistrubutedToPenaltySum
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

        public IActionResult CancelDistributePaymentToAccount(int idPayment)
        {
            try
            {
                var paymentDistributionInfo = dataService.CancelDistributePaymentToAccount(idPayment);
                return Json(new
                {
                    State = "Success",
                    paymentDistributionInfo.Sum,
                    paymentDistributionInfo.DistrubutedToTenancySum,
                    paymentDistributionInfo.DistrubutedToPenaltySum
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

        public IActionResult UploadPayments(List<IFormFile> files)
        {
            var tffStrings = new List<TffString>();
            var kumiPaymentGroupFiles = new List<KumiPaymentGroupFile>();
            List<Tuple<Stream, string>> resultFiles = new List<Tuple<Stream, string>>();
            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file.FileName);
                    if (new string[] { ".zip", ".7z" }.Contains(fileInfo.Extension?.ToLowerInvariant()))
                    {
                        var tmpFile = Path.GetTempFileName();
                        using (var zipStream = new FileStream(tmpFile, FileMode.Truncate))
                        {
                            file.OpenReadStream().CopyTo(zipStream);
                            zipStream.Flush();
                            zipStream.Close();
                        }
                        var archiveFiles = zipArchiveDataService.UnpackRecursive(tmpFile);
                        if (archiveFiles == null) continue;
                        foreach(var archiveFile in archiveFiles)
                        {
                            var archiveFileInfo = new FileInfo(archiveFile);
                            var stream = new FileStream(archiveFile, FileMode.Open);
                            resultFiles.Add(new Tuple<Stream, string>(stream, archiveFileInfo.Name));
                        }
                    } else
                    {
                        resultFiles.Add(new Tuple<Stream, string>(file.OpenReadStream(), file.FileName));
                    }                    
                }
                catch (Exception e)
                {
                    return Error(e.Message);
                }
            }

            foreach(var file in resultFiles)
            {
                try
                {
                    var stream = file.Item1;
                    var tffFileLoader = TffFileLoaderFactory.CreateFileLoader(stream);
                    if (tffFileLoader == null) continue;
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    tffStrings.AddRange(tffFileLoader.Load(stream));
                    stream.Close();
                    kumiPaymentGroupFiles.Add(new KumiPaymentGroupFile
                    {
                        FileName = file.Item2,
                        FileVersion = tffFileLoader.Version
                    });
                }
                catch (BDFormatException e)
                {
                    return Error(string.Format("Файл {0}. " + e.Message, file.Item2));
                }
                catch (Exception e)
                {
                    return Error(e.Message);
                }
            }
            try
            {
                var errorModel = dataService.UploadInfoFromTff(tffStrings, kumiPaymentGroupFiles);
                return View("UploadPaymentsResult", errorModel);
            } catch(ApplicationException e)
            {
                return Error(e.Message);
            }
        }

        public IActionResult DistributePaymentDetails(int idPayment)
        {
            var payment = dataService.GetKumiPayment(idPayment);
            return PartialView("PaymentDistribution", payment);
        }
    }
}