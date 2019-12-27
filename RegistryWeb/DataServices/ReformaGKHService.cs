using Microsoft.Extensions.Configuration;
using RegistryWeb.Models;
using System.Xml.Linq;

namespace RegistryWeb.DataServices
{
    public class ReformaGKHService
    {
        private XNamespace soapenv;
        private XNamespace api;
        private readonly RegistryContext registryContext;
        public string ApiUrl { get; private set; }

        public ReformaGKHService(RegistryContext registryContext, IConfiguration config)
        {
            this.registryContext = registryContext;
            ApiUrl = config.GetValue<string>("ApiUrl");
            soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            api = ApiUrl;
        }

        public string Login(string login, string password)
        {
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
            return xDoc.ToString();
        }

        public string GetReportingPeriodList(string sessionGuid)
        {
            var xDoc = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "api", ApiUrl),
                    new XElement(soapenv + "Header",
                        new XElement("authenticate", sessionGuid)
                    ),
                    new XElement(soapenv + "Body",
                        new XElement(api + "GetReportingPeriodList")
                    )
                )
            );
            return xDoc.ToString();
        }

        public string GetHouseProfileActual(string sessionGuid, int house_id, int reporting_period_id)
        {
            var xDoc = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "api", ApiUrl),
                    new XElement(soapenv + "Header",
                        new XElement("authenticate", sessionGuid)
                    ),
                    new XElement(soapenv + "Body",
                        new XElement(api + "GetHouseProfileActual",
                            new XElement("house_id", house_id),
                            new XElement("reporting_period_id", reporting_period_id)
                        )
                    )
                )
            );
            return xDoc.ToString();
        }
    }
}
