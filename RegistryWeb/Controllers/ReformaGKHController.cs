using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using RegistryWeb.DataServices;
using RegistryWeb.ViewModel;
using System;
using RegistryWeb.Models.Api;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using RegistryWeb.Models;

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

        private XDocument GetResponseSoap(string xmlString)
        {
            var request = WebRequest.Create(reformaGKH.ApiUrl);
            request.Proxy = proxy;
            request.Method = "POST";
            request.ContentType = "application/xml";
            byte[] byteArray = Encoding.UTF8.GetBytes(xmlString);
            request.ContentLength = byteArray.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);

            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var xDoc = XDocument.Load(reader);
            response.Close();

            return xDoc;
        }

        public T Deserialize<T>(XDocument xDoc)
        {
            var serializer = new XmlSerializer(typeof(T));
            var reader = xDoc.CreateReader();
            T result = (T)serializer.Deserialize(reader);
            return result;
        }

        public T Deserialize<T>(string str)
        {
            var xDoc = XDocument.Parse(str);
            return Deserialize<T>(xDoc);
        }

        public XDocument Serialize<T>(T serializeObject)
        {
            var serializer = new XmlSerializer(typeof(T));
            var xDoc = new XDocument();
            using (var writer = xDoc.CreateWriter())
            {
                serializer.Serialize(writer, serializeObject);
            }
            return xDoc;
        }


        public IActionResult Test()
        {
            try
            {
                var entity1 = ApiTest.GetHouseProfileActualResult();
                var xDoc = Serialize<HouseProfileActualResult>(entity1);
                xDoc.Save(@"D:\houseProfileActualResult.xml");
                var entity2 = Deserialize<HouseProfileActualResult>(xDoc);
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
                    var data = reformaGKH.Login(model.User, model.Password);
                    var xDoc = GetResponseSoap(data);
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
                var data = reformaGKH.GetReportingPeriodList(TokenApiStorage.SessionGuid);
                var xDoc = GetResponseSoap(data);
                var list = xDoc.Descendants("GetReportingPeriodListResult").SingleOrDefault();
                xDoc = new XDocument(new XElement("PeriodListResult", list));
                var pr = Deserialize<PeriodListResult>(xDoc);
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
                var data = reformaGKH.GetHouseProfileActual(TokenApiStorage.SessionGuid, 7947873, 465);
                var xDoc = GetResponseSoap(data);
                var list = xDoc.Descendants("GetHouseProfileActualResult").SingleOrDefault();
                xDoc = new XDocument(new XElement("HouseProfileActualResult",
                    new XAttribute(XNamespace.Xmlns + "xsi", list.GetNamespaceOfPrefix("xsi")),
                    list.Elements()));
                var entity = Deserialize<HouseProfileActualResult>(xDoc);
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