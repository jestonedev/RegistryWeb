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

        public ReformaGKHController(ReformaGKHService reformaGKH, SecurityService securityService)
        {
            this.reformaGKH = reformaGKH;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerReadWrite))
                return View("NotAccess");
            ViewData["SessionGuid"] = TokenApiStorage.SessionGuid;
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
            ViewData["SessionGuid"] = TokenApiStorage.SessionGuid;
            return View("Index");
        }

        public IActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    TokenApiStorage.User = model.User;
                    TokenApiStorage.Password = model.Password;
                    var xDoc = reformaGKH.Login(model.User, model.Password);
                    TokenApiStorage.SessionGuid = xDoc.Descendants("LoginResult").Single().Value;
                }
                catch (Exception ex)
                {
                    return Content("Ошибка! \n" + ex.Message);
                }
            }
            ViewData["SessionGuid"] = TokenApiStorage.SessionGuid;
            return View("Index",model);
        }

        public IActionResult GetReportingPeriodList()
        {
            try
            {
                var xDoc = reformaGKH.GetReportingPeriodList(TokenApiStorage.SessionGuid);
                var list = xDoc.Descendants("GetReportingPeriodListResult").SingleOrDefault();
                xDoc = new XDocument(new XElement("PeriodListResult", list));
                var pr = reformaGKH.Deserialize<PeriodListResult>(xDoc);
            }
            catch (Exception ex)
            {
                return Content("Ошибка! \n" + ex.Message);
            }
            ViewData["SessionGuid"] = TokenApiStorage.SessionGuid;
            return View("Index");
        }

        public IActionResult GetHouseProfileActual()
        {
            try
            {
                var xDoc = reformaGKH.GetHouseProfileActual(TokenApiStorage.SessionGuid, 7947873, 465);
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
            ViewData["SessionGuid"] = TokenApiStorage.SessionGuid;
            return View("Index");
        }
    }
}