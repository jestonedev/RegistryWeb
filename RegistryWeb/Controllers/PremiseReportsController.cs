using System;
using System.Collections.Generic;
using System.Globalization;
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
    [HasPrivileges(Privileges.RegistryRead)]
    [DefaultResponseOnException(typeof(Exception))]
    public class PremiseReportsController : SessionController<PremisesListFilter>
    {
        private readonly PremiseReportService reportService;
        private readonly PremiseReportsDataService dataService;
        private readonly SecurityService securityService;

        public PremiseReportsController(PremiseReportService reportService, PremiseReportsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;

            nameFilteredIdsDict = "filteredPremisesIdsDict";
            nameIds = "idPremises";
            nameMultimaster = "PremiseReports";
        }

        public IActionResult GetExcerptPremise(int idPremise, string excerptNumber, DateTime excerptDateFrom, int signer, int excerptHaveLiveSpace)
        {
            var file = reportService.ExcerptPremise(idPremise, excerptNumber, excerptDateFrom, signer, excerptHaveLiveSpace);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Выписка на помещение № {0}.odt", idPremise));
        }

        public IActionResult GetExcerptSubPremise(int idSubPremise, string excerptNumber, DateTime excerptDateFrom, int signer, int excerptHaveLiveSpace)
        {
            var file = reportService.ExcerptSubPremise(idSubPremise, excerptNumber, excerptDateFrom, signer, excerptHaveLiveSpace);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Выписка на комнату № {0}.odt", idSubPremise));
        }

        public IActionResult GetExcerptMunSubPremise(int idPremise, string excerptNumber, DateTime excerptDateFrom, int signer, int excerptHaveLiveSpace)
        {
            if (!dataService.HasMunicipalSubPrmieses(idPremise))
            {
                return Error(string.Format("В помещении № {0} отсутствуют муниципальные комнаты", idPremise));
            }
            var file = reportService.ExcerptMunSubPremises(idPremise, excerptNumber, excerptDateFrom, signer, excerptHaveLiveSpace);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Выписка на мун. комнаты помещения № {0}.odt", idPremise));
        }

        public IActionResult GetPremiseNoticeToBks(int idPremise, string actionText, int paymentType, int signer)
        {
            var file = reportService.PremiseNoticeToBks(idPremise, actionText, paymentType, signer);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Извещение в БКС на помещение № {0}.odt", idPremise));
        }

        public IActionResult GetPremiseNoticeToIes(int idPremise, string actionText, int signer)
        {
            var file = reportService.PremiseNoticeToIes(idPremise, actionText, signer);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Извещение в ИЭСБК на помещение № {0}.odt", idPremise));
        }

        public IActionResult GetSubPremiseNoticeToIes(int idSubPremise, string actionText, int signer)
        {
            var file = reportService.SubPremiseNoticeToIes(idSubPremise, actionText, signer);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Извещение в ИЭСБК на комнату № {0}.odt", idSubPremise));
        }

        public IActionResult GetSubPremiseNoticeToBks(int idSubPremise, string actionText, int paymentType, int signer)
        {
            var file = reportService.SubPremiseNoticeToBks(idSubPremise, actionText, paymentType, signer);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Извещение в БКС на комнату № {0}.odt", idSubPremise));
        }

        public IActionResult GetPremisesArea()
        {
            List<int> ids = GetSessionIds();
            
            if (!ids.Any())
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            var file = reportService.PremisesArea(ids);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Справка о площади помещений.odt"));
        }

        public IActionResult GetPremiseArea(int idPremise)
        {
            var file = reportService.PremisesArea(new List<int> { idPremise });
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Справка о площади помещения № {0}.odt", idPremise));
        }

        //_________________Для массовых____________________ 
        public IActionResult GetMassExcerptPremise(string excerptNumber, DateTime excerptDateFrom, int signer, int excerptHaveLiveSpace)
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();
            
            var file = reportService.ExcerptPremises(ids, excerptNumber, excerptDateFrom, signer, excerptHaveLiveSpace);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Массовая выписка.odt"));
        }

        public IActionResult GetPkBks(int signer, int? idPremise)
        {
            List<int> ids = new List<int>();

            if (idPremise != null)
            {
                ids.Add(idPremise.Value);
            } else
                ids =  GetSessionIds();

            if (!ids.Any())
                return NotFound();
            
            var file = reportService.PkBksPremises(ids, signer);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Запрос ПК в БКС.odt"));
        }

        public IActionResult GetPremisesAct(DateTime actDate, string isNotResides, string commision, int clerk)
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            var file = reportService.MassActPremises(ids, actDate, isNotResides, commision, clerk);
            return File(file, MimeTypeHelper.OdtMime, string.Format(@"Акт о факте проживания.odt"));
        }

        public IActionResult GetPremisesExport()
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();
            
            var file = reportService.ExportPremises(ids);
            return File(file, MimeTypeHelper.OdsMime, string.Format(@"Экспорт данных.ods"));
        }

        [HasPrivileges(Privileges.RegistryRead, Privileges.TenancyRead)]
        public IActionResult GetPremisesTenancyHistory()
        {
            List<int> ids = GetSessionIds();

            if (ids == null)
                return NotFound();

            var idsString = ids
                .Aggregate("", (current, id) => current + id.ToString(CultureInfo.InvariantCulture) + ",").TrimEnd(',');

            var file = reportService.TenancyHistoryPremises(ids);
            return File(file, MimeTypeHelper.OdsMime, string.Format(@"История найма помещений.ods"));
        }
    }
}