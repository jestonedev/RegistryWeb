using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PremiseReportsController : SessionController<PremisesListFilter>
    {
        private readonly PremiseReportService reportService;
        private readonly PremiseReportsDataService dataService;
        private readonly SecurityService securityService;
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";

        public PremiseReportsController(PremiseReportService reportService, PremiseReportsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;

            nameFilteredIdsDict = "filteredPremisesIdsDict";
            nameIds = "idPremises";
            nameMultimaster = "PremiseReports";
        }

        public IActionResult GetExcerptPremise(int idPremise, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.ExcerptPremise(idPremise, excerptNumber, excerptDateFrom, signer);
                return File(file, odsMime, string.Format(@"Выписка на помещение № {0}", idPremise));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetExcerptSubPremise(int idSubPremise, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.ExcerptSubPremise(idSubPremise, excerptNumber, excerptDateFrom, signer);
                return File(file, odsMime, string.Format(@"Выписка на комнату № {0}", idSubPremise));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetExcerptMunSubPremise(int idPremise, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                if (!dataService.HasMunicipalSubPrmieses(idPremise))
                {
                    return Error(string.Format("В помещении № {0} отсутствуют муниципальные комнаты", idPremise));
                }
                var file = reportService.ExcerptMunSubPremises(idPremise, excerptNumber, excerptDateFrom, signer);
                return File(file, odtMime, string.Format(@"Выписка на мун. комнаты помещения № {0}", idPremise));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetPremiseNoticeToBks(int idPremise, string actionText, int paymentType, int signer)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.PremiseNoticeToBks(idPremise, actionText, paymentType, signer);
                return File(file, odtMime, string.Format(@"Извещение в БКС на помещение № {0}", idPremise));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetSubPremiseNoticeToBks(int idSubPremise, string actionText, int paymentType, int signer)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.SubPremiseNoticeToBks(idSubPremise, actionText, paymentType, signer);
                return File(file, odtMime, string.Format(@"Извещение в БКС на комнату № {0}", idSubPremise));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetPremisesArea()
        {
            List<int> ids = GetSessionIds();
            
            if (!ids.Any())
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            try
            {
                var file = reportService.PremisesArea(ids);
                return File(file, odtMime, string.Format(@"Справка о площади помещений"));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetPremiseArea(int idPremise)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.PremisesArea(new List<int> { idPremise });
                return File(file, odtMime, string.Format(@"Справка о площади помещения № {0}", idPremise));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        //_________________Для массовых____________________ 
        public IActionResult GetMassExcerptPremise(string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ExcerptPremises(ids, excerptNumber, excerptDateFrom, signer);
                return File(file, odsMime, string.Format(@"Массовая выписка"));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetPremisesAct(DateTime actDate, string isNotResides, string commision, int clerk)
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            try
            {
                var file = reportService.MassActPremises(ids, actDate, isNotResides, commision, clerk);
                return File(file, odsMime, string.Format(@"Акт о факте проживания"));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetPremisesExport()
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ExportPremises(ids);
                return File(file, odsMime, string.Format(@"Экспорт данных"));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetPremisesTenancyHistory()
        {
            List<int> ids = GetSessionIds();

            if (ids == null)
                return NotFound();

            var idsString = ids
                .Aggregate("", (current, id) => current + id.ToString(CultureInfo.InvariantCulture) + ",").TrimEnd(',');

            if (!securityService.HasPrivilege(Privileges.RegistryRead) || !securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");

            try
            {
                var file = reportService.TenancyHistoryPremises(ids);
                return File(file, odsMime, string.Format(@"История найма помещений"));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}