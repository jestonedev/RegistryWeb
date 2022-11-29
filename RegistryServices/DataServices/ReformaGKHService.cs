using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryReformaGKH;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RegistryWeb.DataServices
{
    public class ReformaGKHService
    {
        private XNamespace soapenv;
        private XNamespace api;
        private readonly RegistryContext registryContext;
        private readonly TokenApiStorage storage;
        public string ApiUrl { get; private set; }
        private WebProxy proxy;

        public ReformaGKHService(RegistryContext registryContext, IConfiguration config, TokenApiStorage storage)
        {
            this.registryContext = registryContext;
            this.storage = storage;
            ApiUrl = config.GetValue<string>("ApiUrl");
            soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            api = ApiUrl;
            proxy = new WebProxy("http://proxy.mcs.br:8080/array.dll?Get.Routing.Script", false)
            {
                UseDefaultCredentials = true
            };
        }

        private XDocument GetResponseSoap(string xmlString)
        {
            var request = WebRequest.Create(ApiUrl);
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

        public XDocument Login(string login, string password)
        {
            storage.User = login;
            storage.Password = password;
            var xDoc = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "api", ApiUrl),
                    new XElement(soapenv + "Header"),
                    new XElement(soapenv + "Body",
                        new XElement(api + "Login",
                            new XElement("login", login),
                            new XElement("password", password)
                        )
                    )
                )
            );
            xDoc = GetResponseSoap(xDoc.ToString());
            storage.SessionGuid = xDoc.Descendants("LoginResult").Single().Value;
            return xDoc;
        }

        public XDocument GetReportingPeriodList()
        {
            var xDoc = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "api", ApiUrl),
                    new XElement(soapenv + "Header",
                        new XElement("authenticate", storage.SessionGuid)
                    ),
                    new XElement(soapenv + "Body",
                        new XElement(api + "GetReportingPeriodList")
                    )
                )
            );
            xDoc = GetResponseSoap(xDoc.ToString());
            return xDoc;
        }

        public XDocument GetHouseProfileActual(int houseId)
        {
            var xDoc = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "api", ApiUrl),
                    new XElement(soapenv + "Header",
                        new XElement("authenticate", storage.SessionGuid)
                    ),
                    new XElement(soapenv + "Body",
                        new XElement(api + "GetHouseProfileActual",
                            new XElement("house_id", houseId)
                        )
                    )
                )
            );
            xDoc = GetResponseSoap(xDoc.ToString());
            return xDoc;
        }
    }
}
