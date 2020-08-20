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
    public class TenancyReportsController : SessionController<TenancyProcessesFilter>
    {
        private readonly TenancyReportService reportService;
        private readonly TenancyReportsDataService dataService;
        private readonly SecurityService securityService;
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";

        public TenancyReportsController(TenancyReportService reportService, TenancyReportsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;

            nameFilteredIdsDict = "filteredTenancyProcessesIdsDict";
            nameIds = "idTenancyProcesses";
            nameMultimaster = "TenancyProcessesReports";
        }

        public IActionResult GetPreContract(int idProcess, int idPreamble, int idCommittee)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                var file = reportService.PreContract(idProcess, idPreamble, idCommittee);
                return File(file, odtMime, string.Format(@"Предварительный договор № {0}", idProcess));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetDksrContract(int idProcess)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                if (!dataService.HasTenant(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан наниматель", idProcess));
                }
                var file = reportService.DksrContract(idProcess);
                return File(file, odtMime, string.Format(@"Договор (ДКСР) № {0}", idProcess));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetContract(int idProcess, int idRentType, int contractType, bool openDate)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasRegistrationDateAndNum(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан номер и/или дата договора найма", idProcess));
                }
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                if (!dataService.HasTenant(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан наниматель", idProcess));
                }
                var file = reportService.Contract(idProcess, idRentType, contractType, openDate);
                return File(file, odtMime, string.Format(@"Договор № {0}", idProcess));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetActToTenant(int idProcess, bool openDate)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasRegistrationDateAndNum(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан номер и/или дата договора найма", idProcess));
                }
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                if (!dataService.HasTenant(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан наниматель", idProcess));
                }
                var file = reportService.Act(idProcess, openDate, 2);
                return File(file, odtMime, string.Format(@"Акт передачи в найм № {0}", idProcess));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetActFromTenant(int idProcess, bool openDate)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasRegistrationDateAndNum(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан номер и/или дата договора найма", idProcess));
                }
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                if (!dataService.HasTenant(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан наниматель", idProcess));
                }
                if (!dataService.HasAgreement(idProcess))
                {
                    return Error(string.Format("В найме {0} отсутствует соглашение", idProcess));
                }
                var file = reportService.Act(idProcess, openDate, 1);
                return File(file, odtMime, string.Format(@"Акт передачи в найм № {0}", idProcess));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetAgreement(int idAgreement)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                var idProcess = dataService.GetProcessIdForAgreement(idAgreement);
                if (!dataService.HasRegistrationDateAndNum(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан номер и/или дата договора найма", idProcess));
                }
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                if (!dataService.HasTenant(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан наниматель", idProcess));
                }
                var file = reportService.Agreement(idAgreement);
                return File(file, odtMime, string.Format(@"Соглашение найма № {0}", idProcess));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetNotifySingleDocument(int idProcess, int reportType, string reportTitle)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                if (new int[] { 1, 7 }.Contains(reportType) && !dataService.HasRegistrationDateAndNum(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан номер и/или дата договора найма", idProcess));
                }
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                if (!dataService.HasTenant(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан наниматель", idProcess));
                }
                var file = reportService.NotifySingleDocument(idProcess, reportType);
                return File(file, odtMime, string.Format(@reportTitle+" (найм № {0})", idProcess));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetRequestToMvd(int idProcess, int requestType)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                if (!dataService.HasTenancies(idProcess))
                {
                    return Error(string.Format("В найме {0} отсутствуют участники", idProcess));
                }
                var file = reportService.RequestToMvd(idProcess, requestType);
                return File(file, odtMime, string.Format("Запрос в МВД (найм № {0})", idProcess));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        //_________________Для массовых____________________ 
        private IActionResult GetNotifies(TenancyNotifiesReportTypeEnum reportType, string fileName)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                List<int> ids = GetSessionIds();
                var file = reportService.GetNotifies(ids, reportType);
                return File(file, odtMime, string.Format(fileName));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetNotifiesPrimary()
        {
            return GetNotifies(TenancyNotifiesReportTypeEnum.PrintNotifiesPrimary, @"Первичное уведомление");
        }

        public IActionResult GetNotifiesSecondary()
        {
            return GetNotifies(TenancyNotifiesReportTypeEnum.PrintNotifiesSecondary, @"Повторное уведомление");
        }
        public IActionResult GetNotifiesProlongContract()
        {
            return GetNotifies(TenancyNotifiesReportTypeEnum.PrintNotifiesProlongContract, @"Ответ на обращение по продлению");
        }

        public IActionResult GetNotifiesEvictionFromEmergencyFund()
        {
            return GetNotifies(TenancyNotifiesReportTypeEnum.PrintNotifiesEvictionFromEmergencyFund, @"Уведомление о выселении из АФ");
        }
    }
}