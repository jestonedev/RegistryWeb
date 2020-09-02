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

            nameFilteredIdsDict = "filteredClaimsIdsDict";
            nameIds = "idClaims";
            nameMultimaster = "ClaimsReports";
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
                var idAccounts = dataService.GetAccountIds(processingIds);
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