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

namespace RegistryWeb.Controllers
{
    public class ReformaGKHController : Controller
    {        
        private string sessionGuid;
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
            return View();
        }

        public IActionResult LoginIn(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var request = WebRequest.Create(reformaGKH.ApiUrl);
                    request.Proxy = proxy;
                    request.Method = "POST";
                    string data = reformaGKH.GetXmlLogin(model.User, model.Password);
                    byte[] byteArray = Encoding.UTF8.GetBytes(data);
                    request.ContentType = "application/xml";
                    request.ContentLength = byteArray.Length;
                    var dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);

                    var response = (HttpWebResponse)request.GetResponse();
                    var stream = response.GetResponseStream();
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    var xDoc = XDocument.Load(reader);
                    sessionGuid = xDoc.Descendants("LoginResult").Single().Value;
                    response.Close();
                    return Content(sessionGuid);
                }
                catch (Exception ex)
                {
                    return Content("Ошибка! \n" + ex.Message);
                }
            }
            return View(model);
        }
    }
}