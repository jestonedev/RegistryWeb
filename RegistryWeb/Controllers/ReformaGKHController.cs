using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;
using System.Xml.Linq;
using System.Linq;
using RegistryWeb.DataServices;
using RegistryWeb.ViewModel;
using System;
using RegistryWeb.Models.Api;
using RegistryWeb.Models;

namespace RegistryWeb.Controllers
{
    public class ReformaGKHController : Controller
    {
        private readonly ReformaGKHService reformaGKH;
        private readonly SecurityService securityService;
        private readonly TokenApiStorage storage;

        public ReformaGKHController(ReformaGKHService reformaGKH, SecurityService securityService, TokenApiStorage storage)
        {
            this.reformaGKH = reformaGKH;
            this.securityService = securityService;
            this.storage = storage;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerReadWrite))
                return View("NotAccess");
            ViewData["SessionGuid"] = storage.SessionGuid;
            return View();
        }

        public IActionResult Test()
        {
            try
            {
                var entity1 = ApiTest.GetHouseProfileActualResult();
                var xDoc = reformaGKH.Serialize<HouseProfileActualResult>(entity1);
                xDoc.Save(@"D:\houseProfileActualResult.xml");
                var entity2 = reformaGKH.Deserialize<HouseProfileActualResult>(xDoc);
            }
            catch (Exception ex)
            {
                return Content("Ошибка! \n" + ex.Message);
            }
            ViewData["SessionGuid"] = storage.SessionGuid;
            return View("Index");
        }

        public IActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var xDoc = reformaGKH.Login(model.User, model.Password);
                }
                catch (Exception ex)
                {
                    return Content("Ошибка! \n" + ex.Message);
                }
            }
            ViewData["SessionGuid"] = storage.SessionGuid;
            return View("Index",model);
        }

        public IActionResult GetReportingPeriodList()
        {
            try
            {
                var xDoc = reformaGKH.GetReportingPeriodList();
                var list = xDoc.Descendants("GetReportingPeriodListResult").SingleOrDefault();
                xDoc = new XDocument(new XElement("PeriodListResult", list));
                var pr = reformaGKH.Deserialize<PeriodListResult>(xDoc);
            }
            catch (Exception ex)
            {
                return Content("Ошибка! \n" + ex.Message);
            }
            ViewData["SessionGuid"] = storage.SessionGuid;
            return View("Index");
        }

        public IActionResult GetHouseProfileActual()
        {
            try
            {
                var xDoc = reformaGKH.GetHouseProfileActual(8939273);
                var list = xDoc.Descendants("GetHouseProfileActualResult").SingleOrDefault();
                xDoc = new XDocument(new XElement("HouseProfileActualResult",
                    new XAttribute(XNamespace.Xmlns + "xsi", list.GetNamespaceOfPrefix("xsi")),
                    list.Elements()));
                var entity = reformaGKH.Deserialize<HouseProfileActualResult>(xDoc);
            }
            catch (Exception ex)
            {
                return Content("Ошибка! \n" + ex.Message);
            }
            ViewData["SessionGuid"] = storage.SessionGuid;
            return View("Index");
        }
    }
}