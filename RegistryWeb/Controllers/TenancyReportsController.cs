using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryServices.DataHelpers;
using RegistryWeb.DataServices;
using RegistryWeb.Filters;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.TenancyRead)]
    [DefaultResponseOnException(typeof(Exception))]
    public class TenancyReportsController : SessionController<TenancyProcessesFilter>
    {
        private readonly TenancyReportService reportService;
        private readonly TenancyReportsDataService dataService;
        private readonly SecurityService securityService;

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
            if (!dataService.HasRentObjects(idProcess))
            {
                return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
            }
            var file = reportService.PreContract(idProcess, idPreamble, idCommittee);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Предварительный договор № {0}.odt", idProcess));
        }

        public IActionResult GetDksrContract(int idProcess)
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
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Договор (ДКСР) № {0}.odt", idProcess));
        }

        public IActionResult GetFreeUseContract(int idProcess)
        {
            if (!dataService.HasRentObjects(idProcess))
            {
                return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
            }
            if (!dataService.HasTenant(idProcess))
            {
                return Error(string.Format("В найме {0} не указан наниматель", idProcess));
            }
            var file = reportService.FreeUseContract(idProcess);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Договор БП № {0}.odt", idProcess));
        }

        public IActionResult GetFreeUseAct(int idProcess)
        {
            if (!dataService.HasRentObjects(idProcess))
            {
                return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
            }
            if (!dataService.HasTenant(idProcess))
            {
                return Error(string.Format("В найме {0} не указан наниматель", idProcess));
            }
            var file = reportService.FreeUseAct(idProcess);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Акт приема-передачи БП № {0}.odt", idProcess));
        }

        public IActionResult GetStatementResettle(int idProcess)
        {
            if (!dataService.HasRentObjects(idProcess))
            {
                return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
            }
            if (!dataService.HasTenant(idProcess))
            {
                return Error(string.Format("В найме {0} не указан наниматель", idProcess));
            }
            var file = reportService.StatementResettleSecondary(idProcess);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Заявление на переселение Вторичка № {0}.odt", idProcess));
        }

        public IActionResult GetContract(int idProcess, int idRentType, int contractType, bool openDate)
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
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Договор № {0}.odt", idProcess));
        }

        public IActionResult GetActToTenant(int idProcess, bool openDate)
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
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Акт передачи в найм № {0}.odt", idProcess));
        }

        public IActionResult GetActFromTenant(int idProcess, bool openDate)
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
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Акт передачи в найм № {0}.odt", idProcess));
        }

        public IActionResult GetActAf(int idProcess, int idPreparer)
        {
            if (!dataService.HasRentObjects(idProcess))
            {
                return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
            }
            if (!dataService.HasTenant(idProcess))
            {
                return Error(string.Format("В найме {0} не указан наниматель", idProcess));
            }
            var file = reportService.ActAf(idProcess, idPreparer);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Акт приема-передачи (АФ) № {0}.odt", idProcess));
        }

        public IActionResult GetAgreement(int idAgreement)
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
                return File(file, MimeTypeHelper.OdtMime, string.Format(@"Соглашение найма № {0}.odt", idProcess));
        }

        public IActionResult GetAgreementReady(int idAgreement)
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
            var file = reportService.AgreementReady(idProcess);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Уведомление о готовности соглашения найма № {0}.odt", idProcess));
        }

        public IActionResult GetNotifySingleDocument(int idProcess, int reportType, string reportTitle)
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
            return File(file, MimeTypeHelper.OdtMime, string.Format(@reportTitle+" (найм № {0}).odt", idProcess));
        }

        public IActionResult GetRequestToMvd(int requestType, int idProcess = 0)
        {
            List<int> ids = new List<int>();
            var processingIds = new List<int>();
            var errorIds = new List<int>();
            var fileName = @"Запрос в МВД";
            if (requestType == 2)
            {
                fileName = @"Запрос в МВД - новый шаблон";
            }
            if (idProcess == 0)
            {
                ids = GetSessionIds();
                foreach (var id in ids)
                {
                    if (!dataService.HasRentObjects(id))
                    {
                        errorIds.Add(id);
                        continue;
                    }
                    if (!dataService.HasTenancies(id))
                    {
                        errorIds.Add(id);
                        continue;
                    }
                    processingIds.Add(id);
                }
                if (errorIds.Any())
                {
                    return Error(string.Format("В найм{1} {0} не указан адрес нанимаемого жилья или отсутствуют участники",
                        errorIds.Select(r => r.ToString()).Aggregate((acc, v) => acc + ", " + v),
                        errorIds.Count == 1 ? "е" : "ах"));
                }
            }
            else
            {
                if (!dataService.HasRentObjects(idProcess))
                {
                    return Error(string.Format("В найме {0} не указан адрес нанимаемого жилья", idProcess));
                }
                if (!dataService.HasTenancies(idProcess))
                {
                    return Error(string.Format("В найме {0} отсутствуют участники", idProcess));
                }
                processingIds.Add(idProcess);
                fileName += string.Format(" (найм № {0}).odt", idProcess);
            }
            var file = reportService.RequestToMvd(processingIds, requestType);
            return File(file, MimeTypeHelper.OdtMime, fileName);
        }

        private IActionResult GetNotifies(TenancyNotifiesReportTypeEnum reportType, string fileName)
        {
            var ids = GetSessionIds();
            var file = reportService.Notifies(ids, reportType);
            return File(file, MimeTypeHelper.OdtMime, string.Format("{0}.odt", fileName));
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

        public IActionResult GetTenancyWarning(int idPreparer, bool isMultipageDocument)
        {
            var ids = GetSessionIds();
            var file = reportService.TenancyWarning(ids, idPreparer, isMultipageDocument);
            if (isMultipageDocument)
                return File(file, MimeTypeHelper.OdtMime, @"Предупреждения.odt");
            return File(file, MimeTypeHelper.ZipMime, @"Предупреждения.zip"); 
        }

        [HttpGet]
        [HasPrivileges(Privileges.TenancyWrite)]
        [DefaultResponseOnException(typeof(Exception), ViewResultType = typeof(JsonResult))]
        public IActionResult SetTenancyContractRegDate(DateTime regDate)
        {
            var ids = GetSessionIds();
            var noValidateContracts = dataService.GetNoValidateContracts(ids);

            if (noValidateContracts.Any())
            {
                var message = @"Для процессов найма №" +
                    noValidateContracts.Select(id => id.ToString()).Aggregate((x, y) => x + ", " + y) +
                    "не проставлен номер договора. Для проставления даты регистрации номер должен быть присвоен!";
                return Json(message);
            }

            dataService.SetTenancyContractRegDate(ids, regDate);
            return Json("Для всех процессов найма дата регистрации была успешно присвоена!");
        }

        [HttpGet]
        [HasPrivileges(Privileges.TenancyWrite)]
        [DefaultResponseOnException(typeof(Exception), ViewResultType = typeof(JsonResult))]
        public IActionResult SetTenancyReason(string reasonNumber, DateTime reasonDate, int idReasonType, bool isDeletePrevReasons)
        {
            var ids = GetSessionIds();
            dataService.SetTenancyReason(ids, reasonNumber, reasonDate, idReasonType, isDeletePrevReasons);
            return Json("Для всех процессов найма проставление документа-основания успешно завершено!");
        }

        public IActionResult GetExportReasonsForGisZkh()
        {
            var ids = GetSessionIds();
            var file = reportService.ExportReasonsForGisZkh(ids);
            return File(file, MimeTypeHelper.ZipMime, "Документ-основания для ГИС \"ЖКХ\".zip");
        }

        public IActionResult GetGisZkhExport()
        {
            var ids = GetSessionIds();
            var file = reportService.GisZkhExport(ids);
            return File(file, MimeTypeHelper.XlsxMime, "Экспорт для ГИС \"ЖКХ\".xlsx"); 
        }

        public IActionResult GetTenanciesExport()
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            var file = reportService.TenanciesExport(ids);
            return File(file, MimeTypeHelper.OdsMime, string.Format(@"Экспорт данных.ods"));
        }

        public IActionResult GetNoticeToBks(int idProcess, string actionText, int paymentType, int signer)
        {
            var file = reportService.NoticeToBks(idProcess, actionText, paymentType, signer);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Извещение в БКС по найму № {0}.odt", idProcess));
        }

        public IActionResult GetNoticeToIes(int idProcess, string actionText, int signer)
        {
            var file = reportService.NoticeToIes(idProcess, actionText, signer);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Извещение в ИЭСБК по найму № {0}.odt", idProcess));
        }
    }
}