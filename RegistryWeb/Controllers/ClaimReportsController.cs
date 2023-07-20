using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class ClaimReportsController : SessionController<ClaimsFilter>
    {
        private readonly ClaimReportService reportService;
        private readonly ClaimReportsDataService dataService;
        private readonly ClaimsDataService claimsDataService;
        private readonly SecurityService securityService;
        private const string zipMime = "application/zip";
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";
        private const string xlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string docxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public ClaimReportsController(ClaimReportService reportService, ClaimReportsDataService dataService, SecurityService securityService,
             ClaimsDataService claimsDataService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;
            this.claimsDataService = claimsDataService;
            nameFilteredIdsDict = "filteredClaimsIdsDict";
            nameIds = "idClaims";
            nameMultimaster = "ClaimsReports";
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            var viewModel = dataService.GetViewModel();

            return View("Index", viewModel);
        }

        public IActionResult GetTransferToLegal(int idClaim, int idSigner, DateTime dateValue)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            try
            {
                List<int> ids = new List<int>();
                var processingIds = new List<int>();
                var errorIds = new List<int>();
                if (idClaim == 0)
                {
                    ids = GetSessionIds();
                    foreach (var id in ids)
                    {
                        if (!dataService.HasClaimState(id, 2))
                        {
                            errorIds.Add(id);
                            continue;
                        }
                        processingIds.Add(id);
                    }
                    if (errorIds.Any())
                    {
                        return Error(string.Format("В исков{1} работ{2} {0} отсутствует стадия передачи в юридический отдел", 
                            errorIds.Select(r => r.ToString()).Aggregate((acc, v) => acc+", "+v),
                            errorIds.Count == 1 ? "ой" : "ых", errorIds.Count == 1 ? "е" : "ах"));
                    }
                }
                else
                {
                    if (!dataService.HasClaimState(idClaim, 2))
                    {
                        return Error(string.Format("В исковой работе {0} отсутствует стадия передачи в юридический отдел", idClaim));
                    }
                    processingIds.Add(idClaim);
                }
                var file = reportService.TransferToLegal(processingIds, idSigner, dateValue);
                return File(file, docxMime, string.Format(@"Передача в юр. отдел{0}.docx", idClaim == 0 ? "" : string.Format(" (исковой работы № {0})", idClaim)));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetRequestToBks(int idClaim, int idSigner, DateTime dateValue, int idReportBKSType)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            try
            {
                List<int> ids = new List<int>();
                var processingIds = new List<int>();
                var errorIds = new List<int>();
                if (idClaim == 0)
                {
                    ids = GetSessionIds();
                    foreach (var id in ids)
                    {
                        if (!dataService.HasClaimState(id, 1))
                        {
                            errorIds.Add(id);
                            continue;
                        }
                        processingIds.Add(id);
                    }
                    if (errorIds.Any())
                    {
                        return Error(string.Format("В исков{1} работ{2} {0} отсутствует стадия запроса в БКС",
                            errorIds.Select(r => r.ToString()).Aggregate((acc, v) => acc + ", " + v),
                            errorIds.Count == 1 ? "ой" : "ых", errorIds.Count == 1 ? "е" : "ах"));
                    }
                }
                else
                {
                    if (!dataService.HasClaimState(idClaim, 1))
                    {
                        return Error(string.Format("В исковой работе {0} отсутствует стадия запроса в БКС", idClaim));
                    }
                    processingIds.Add(idClaim);
                }

                var file = reportService.RequestToBks(processingIds, idSigner, dateValue, idReportBKSType);
                return File(file, odtMime, string.Format(@"Запрос в БКС{0}.odt", idClaim == 0 ? "" : string.Format(" (иск. работа № {0})", idClaim)));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetCourtOrderStatement(int idClaim, int idOrder)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            try
            {
                var file = reportService.CourtOrderStatement(idClaim, idOrder);
                return File(file, odtMime, string.Format(@"Заявление о выдаче судебного приказа (иск. работа № {0}).odt", idClaim));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimsExport()
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ExportClaims(ids);
                return File(file, odsMime, "Экспорт данных.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimsForDoverie(int statusSending)
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimsForDoverie(ids, statusSending);
                return File(file, odsMime, "обменный файл АИС 'Доверие'.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetSplitAccountsReport()
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.SplitAccountsReport();
                return File(file, odsMime, "Статистика по разделенным лицевым счетам.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimStatesReport(DateTime startDate, DateTime endDate, int idStateType, bool isCurrentState)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimStatesReport(startDate, endDate, idStateType, isCurrentState);
                return File(file, odsMime, "Отчет по стадиям исковых работ.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimStatesAllDatesReport(DateTime startDate, DateTime endDate, int idStateType, bool isCurrentState)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimStatesAllDatesReport(startDate, endDate, idStateType, isCurrentState);
                return File(file, odsMime, "Отчет по стадиям исковых работ.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimExecutorsReport(DateTime startDate, DateTime endDate, int idExecutor)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var executor = dataService.GetExecutor(idExecutor);
                var file = reportService.ClaimExecutorReport(startDate, endDate, executor);
                return File(file, odsMime, "Отчет по исполнителям исковой работы.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimCourtReport(DateTime startDate, DateTime endDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimCourtReport(startDate, endDate);
                return File(file, odsMime, "Подготовленные судебные приказы.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetCourtOspStatement(int idClaim, DateTime createDate, int idSigner)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                claimsDataService.ClaimLogCourtOsp(idClaim);
                var personsCount = claimsDataService.ReceivePersonCount(idClaim);
                var file = reportService.ClaimCourtOspReport(idClaim, createDate, personsCount, idSigner);

                return File(file, odtMime, string.Format("Заявление о возбуждении ИП (иск. работа № {0}).odt", idClaim));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetCourtSpiStatement(int idClaim, int idCourtType)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimCourtSpiReport(idClaim, idCourtType);
                return File(file, odtMime, string.Format("Заявление о прекращении ИП (иск. работа № {0}).odt", idClaim));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimEmergencyTariffReport(DateTime startDate, DateTime endDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimEmergencyTariffReport(startDate, endDate);
                return File(file, xlsxMime, "Отчет по тарифам аварийных помещений.xlsx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimFactMailing(int flag, DateTime startDate, DateTime endDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimFactMailingReport(flag, startDate, endDate);
                return File(file, odsMime, "Отчет по факту рассылки.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimDateReferralCcoBailiffs(DateTime startDate, DateTime endDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimDateReferralCcoBailiffs(startDate, endDate);
                return File(file, odsMime, "Отчет по дате направления с/п приставам.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetClaimExecutedWork(DateTime startDate, DateTime endDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ClaimExecutedWork(startDate, endDate);
                return File(file, xlsxMime, string.Format("Отчет о проделанной работе {0}-{1}.xlsx", startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy")));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetUkInvoiceAgg(List<int> idsOrganization)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.UkInvoiceAgg(idsOrganization);
                return File(file, docxMime, string.Format("Накладная на отправку счетов от {0}.docx", DateTime.Now.ToString("dd.MM.yyyy")));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetUkInvoiceDetails(List<int> idsOrganization)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.UkInvoiceDetails(idsOrganization);
                return File(file, xlsxMime, string.Format("Детализация по накладной на отправку счетов от {0}.xlsx", DateTime.Now.ToString("dd.MM.yyyy")));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetPaymentsForPeriod(DateTime startDate, DateTime endDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.PaymentsForPeriod(startDate, endDate);
                return File(file, odsMime, string.Format("Платежи КБК найма за период {0}-{1}.ods", startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy")));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetBalanceForPeriod(DateTime startDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var paymentDate = new DateTime(startDate.Year, startDate.Month, 1);
                paymentDate = paymentDate.AddMonths(1).AddDays(-1);

                var file = reportService.BalanceForPeriod(paymentDate);
                return File(file, odsMime, string.Format("Начисления ЛС КУМИ за период {0}-{1}.ods", startDate.ToString("dd.MM.yyyy"), paymentDate.ToString("dd.MM.yyyy")));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}