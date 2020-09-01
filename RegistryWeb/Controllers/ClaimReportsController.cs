using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly SecurityService securityService;
        private const string zipMime = "application/zip";
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";
        private const string xlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string docxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public ClaimReportsController(ClaimReportService reportService, ClaimReportsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;
        }

        public IActionResult GetTransferToLegal(int idClaim, int idSigner, DateTime dateValue)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasClaimState(idClaim, 2))
                {
                    return Error(string.Format("В исковой работе {0} отсутствует стадия передачи в юридический отдел", idClaim));
                }
                var file = reportService.TransferToLegal(new List<int> { idClaim }, idSigner, dateValue);
                return File(file, docxMime, string.Format(@"Передача в юр. отдел исковой работы № {0}", idClaim));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetRequestToBks(int idClaim, int idSigner, DateTime dateValue)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasClaimState(idClaim, 1))
                {
                    return Error(string.Format("В исковой работе {0} отсутствует стадия запроса в БКС", idClaim));
                }
                var idAccounts = dataService.GetAccountIds(new List<int> { idClaim });
                var file = reportService.RequestToBks(idAccounts, idSigner, dateValue);
                return File(file, odtMime, string.Format(@"Запрос в БКС (иск. работа № {0})", idClaim));
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
                return File(file, odtMime, string.Format(@"Заявление о выдаче судебного приказа (иск. работа № {0})", idClaim));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}