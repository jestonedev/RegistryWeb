using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using RegistryWeb.DataServices;
using RegistryWeb.ViewModel;
using System;
using RegistryWeb.Models.Api;

namespace RegistryWeb.Controllers
{
    public class ReformaGKHController : Controller
    {
        private readonly ReformaGKHService reformaGKH;
        private readonly SecurityService securityService;
        private WebProxy proxy;

        public ReformaGKHController(ReformaGKHService reformaGKH, SecurityService securityService)
        {
            this.reformaGKH = reformaGKH;
            this.securityService = securityService;
            proxy = new WebProxy("http://proxy.mcs.br:8080/array.dll?Get.Routing.Script", false)
            {
                UseDefaultCredentials = true
            };
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerReadWrite))
                return View("NotAccess");
            ViewData["SessionGuid"] = TokenApiStorage.SessionGuid;
            return View();
        }

        private WebRequest sendRequest(string xmlString)
        {
            var request = WebRequest.Create(reformaGKH.ApiUrl);
            request.Proxy = proxy;
            request.Method = "POST";
            request.ContentType = "application/xml";
            byte[] byteArray = Encoding.UTF8.GetBytes(xmlString);
            request.ContentLength = byteArray.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            return request;
        }

        public IActionResult LoginIn(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    TokenApiStorage.User = model.User;
                    TokenApiStorage.Password = model.Password;
                    var loginXmlStr = reformaGKH.Login(model.User, model.Password);
                    var request = sendRequest(loginXmlStr);

                    var response = (HttpWebResponse)request.GetResponse();
                    var stream = response.GetResponseStream();
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    var xDoc = XDocument.Load(reader);
                    TokenApiStorage.SessionGuid = xDoc.Descendants("LoginResult").Single().Value;
                    response.Close();
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
                var data = reformaGKH.GetReportingPeriodList(TokenApiStorage.SessionGuid);
                var request = sendRequest(data);

                var response = (HttpWebResponse)request.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream, Encoding.UTF8);
                var xDoc = XDocument.Load(reader);
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