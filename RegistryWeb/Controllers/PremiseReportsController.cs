using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    public class PremiseReportsController : RegistryBaseController
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

        public IActionResult GetPremiseArea(int idPremise)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.PremiseArea(idPremise);
                return File(file, odtMime, string.Format(@"Справка о площади помещения № {0}", idPremise));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}