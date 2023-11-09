using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryServices.DataHelpers;
using RegistryWeb.DataServices.Claims;
using RegistryWeb.Filters;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.ClaimsRead, Privileges.AccountsRead, PrivilegesComparator = Filters.Common.PrivilegesComparator.Or)]
    [DefaultResponseOnException(typeof(Exception))]
    public class ClaimReportsController : SessionController<ClaimsFilter>
    {
        private readonly ClaimReportService reportService;
        private readonly ClaimReportsDataService dataService;
        private readonly ClaimsDataService claimsDataService;
        private readonly SecurityService securityService;

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
            var viewModel = dataService.GetViewModel();

            return View("Index", viewModel);
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetTransferToLegal(int idClaim, int idSigner, DateTime dateValue)
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
                        errorIds.Select(r => r.ToString()).Aggregate((acc, v) => acc + ", " + v),
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
            return File(file, MimeTypeHelper.DocxMime, string.Format(@"Передача в юр. отдел{0}.docx", idClaim == 0 ? "" : string.Format(" (исковой работы № {0})", idClaim)));
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetRequestToBks(int idClaim, int idSigner, DateTime dateValue, int idReportBKSType)
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
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Запрос в БКС{0}.odt", idClaim == 0 ? "" : string.Format(" (иск. работа № {0})", idClaim)));
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetCourtOrderStatement(int idClaim, int idOrder)
        {
            var file = reportService.CourtOrderStatement(idClaim, idOrder);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Заявление о выдаче судебного приказа (иск. работа № {0}).odt", idClaim));
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimsExport()
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            var file = reportService.ExportClaims(ids);
            return File(file, MimeTypeHelper.OdsMime, "Экспорт данных.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimsForDoverie(int statusSending)
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            var file = reportService.ClaimsForDoverie(ids, statusSending);
            return File(file, MimeTypeHelper.OdsMime, "обменный файл АИС 'Доверие'.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetSplitAccountsReport()
        {
            var file = reportService.SplitAccountsReport();
            return File(file, MimeTypeHelper.OdsMime, "Статистика по разделенным лицевым счетам.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimStatesReport(DateTime startDate, DateTime endDate, int idStateType, bool isCurrentState)
        {
            var file = reportService.ClaimStatesReport(startDate, endDate, idStateType, isCurrentState);
            return File(file, MimeTypeHelper.OdsMime, "Отчет по стадиям исковых работ.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimStatesAllDatesReport(DateTime startDate, DateTime endDate, int idStateType, bool isCurrentState)
        {
            var file = reportService.ClaimStatesAllDatesReport(startDate, endDate, idStateType, isCurrentState);
            return File(file, MimeTypeHelper.OdsMime, "Отчет по стадиям исковых работ.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimExecutorsReport(DateTime startDate, DateTime endDate, int idExecutor)
        {
            var executor = dataService.GetExecutor(idExecutor);
            var file = reportService.ClaimExecutorReport(startDate, endDate, executor);
            return File(file, MimeTypeHelper.OdsMime, "Отчет по исполнителям исковой работы.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimCourtReport(DateTime startDate, DateTime endDate)
        {
            var file = reportService.ClaimCourtReport(startDate, endDate);
            return File(file, MimeTypeHelper.OdsMime, "Подготовленные судебные приказы.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetCourtOspStatement(int idClaim, DateTime createDate, int idSigner)
        {
            claimsDataService.ClaimLogCourtOsp(idClaim);
            var personsCount = claimsDataService.ReceivePersonCount(idClaim);
            var file = reportService.ClaimCourtOspReport(idClaim, createDate, personsCount, idSigner);
            return File(file, MimeTypeHelper.OdtMime, string.Format("Заявление о возбуждении ИП (иск. работа № {0}).odt", idClaim));
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetCourtSpiStatement(int idClaim, int idCourtType)
        {
            var file = reportService.ClaimCourtSpiReport(idClaim, idCourtType);
            return File(file, MimeTypeHelper.OdtMime, string.Format("Заявление о прекращении ИП (иск. работа № {0}).odt", idClaim));
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimEmergencyTariffReport(DateTime startDate, DateTime endDate)
        {
            var file = reportService.ClaimEmergencyTariffReport(startDate, endDate);
            return File(file, MimeTypeHelper.XlsxMime, "Отчет по тарифам аварийных помещений.xlsx");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimFactMailing(int flag, DateTime startDate, DateTime endDate)
        {
            var file = reportService.ClaimFactMailingReport(flag, startDate, endDate);
            return File(file, MimeTypeHelper.OdsMime, "Отчет по факту рассылки.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimDateReferralCcoBailiffs(DateTime startDate, DateTime endDate)
        {
            var file = reportService.ClaimDateReferralCcoBailiffs(startDate, endDate);
            return File(file, MimeTypeHelper.OdsMime, "Отчет по дате направления с/п приставам.ods");
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetClaimExecutedWork(DateTime startDate, DateTime endDate)
        {
            var file = reportService.ClaimExecutedWork(startDate, endDate);
            return File(file, MimeTypeHelper.XlsxMime, string.Format("Отчет о проделанной работе {0}-{1}.xlsx", startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy")));
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetUkInvoiceAgg(List<int> idsOrganization)
        {
            var file = reportService.UkInvoiceAgg(idsOrganization);
            return File(file, MimeTypeHelper.DocxMime, string.Format("Накладная на отправку счетов от {0}.docx", DateTime.Now.ToString("dd.MM.yyyy")));
        }

        [HasPrivileges(Privileges.ClaimsRead)]
        public IActionResult GetUkInvoiceDetails(List<int> idsOrganization)
        {
            var file = reportService.UkInvoiceDetails(idsOrganization);
            return File(file, MimeTypeHelper.XlsxMime, string.Format("Детализация по накладной на отправку счетов от {0}.xlsx", DateTime.Now.ToString("dd.MM.yyyy")));
        }

        [HasPrivileges(Privileges.AccountsRead)]
        public IActionResult GetUkInvoiceAggKumi()
        {
            var file = reportService.UkInvoiceAggKumi();
            return File(file, MimeTypeHelper.DocxMime, string.Format("Накладная на отправку счетов от {0}.docx", DateTime.Now.ToString("dd.MM.yyyy")));
        }


        [HasPrivileges(Privileges.AccountsRead)]
        public IActionResult GetUkInvoiceDetailsKumi()
        {
            var file = reportService.UkInvoiceDetailsKumi();
            return File(file, MimeTypeHelper.XlsxMime, string.Format("Детализация по накладной на отправку счетов от {0}.xlsx", DateTime.Now.ToString("dd.MM.yyyy")));
        }


        [HasPrivileges(Privileges.AccountsRead)]
        public IActionResult GetBalanceForPeriod(DateTime startDate, int reportType)
        {
            var paymentDate = new DateTime(startDate.Year, startDate.Month, 1);
            paymentDate = paymentDate.AddMonths(1).AddDays(-1);

            var file = reportService.BalanceForPeriod(paymentDate, reportType);
            return File(file, MimeTypeHelper.OdsMime, string.Format("Начисления ЛС КУМИ за период {0}-{1}.ods", startDate.ToString("dd.MM.yyyy"), paymentDate.ToString("dd.MM.yyyy")));
        }

        [HasPrivileges(Privileges.AccountsRead)]
        public IActionResult GetSberbankFile(DateTime startDate)
        {
            var paymentDate = new DateTime(startDate.Year, startDate.Month, 1);
            paymentDate = paymentDate.AddMonths(1).AddDays(-1);

            var file = reportService.SberbankFile(paymentDate);
            var monthStr = (startDate.Month < 10 ? "0" : "") + startDate.Month.ToString();
            return File(file, MimeTypeHelper.CsvMime, string.Format("18018001_3803201800_40101810900000010001_{0}_y{1}.csv", monthStr, (startDate.Year % 100).ToString()));
        }


        [HasPrivileges(Privileges.AccountsRead)]
        public IActionResult GetPaymentsForPeriod(DateTime startDate, DateTime endDate, string kbk)
        {
            var file = reportService.PaymentsForPeriod(startDate, endDate, kbk);
            var kbkName = "";
            switch(kbk)
            {
                case "90111109044041000120":
                    kbkName= "найма";
                    break;
                case "90111705040041111180":
                    kbkName = "ДГИ";
                    break;
            }
            return File(file, "application/vnd.ms-excel", string.Format("Платежи КБК {0} за период {1}-{2}.xls", kbkName, startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy")));
        }


        [HasPrivileges(Privileges.AccountsRead)]
        public IActionResult GetFileForDoverie(DateTime startDate, bool excludeUploaded, bool saveUploadFact)
        {
            var date = new DateTime(startDate.Year, startDate.Month, 1);
            date = date.AddMonths(1).AddDays(-1);

            var file = reportService.FileForDoverie(date, excludeUploaded, saveUploadFact);
            return File(file, MimeTypeHelper.OdsMime, string.Format("Обменный файл для АИС \"Доверие\" от {0}.ods", DateTime.Now.ToString("dd.MM.yyyy")));
        }

        [HasPrivileges(Privileges.AccountsRead)]
        public IActionResult GetReconciliationPaymentsForPeriod(DateTime startDate, DateTime endDate, DateTime forPeriod)
        {
            var kbk = "90111109044041000120";
            var file = reportService.ReconciliationPaymentsForPeriod(startDate, endDate, forPeriod, kbk);
            var kbkName = "";
            switch (kbk)
            {
                case "90111109044041000120":
                    kbkName = "найма";
                    break;
                case "90111705040041111180":
                    kbkName = "ДГИ";
                    break;
            }

            return File(file, MimeTypeHelper.OdsMime, string.Format("Отчет-сверка по платежам {0} за {3} за период с {1}-{2}.ods", kbkName, startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy"), forPeriod.ToString("MMMM")));
        }
    }
}