using Microsoft.Extensions.Configuration;
using RegistryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public string GetXmlLogin(string login, string password)
        {
            var xDoc = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", "http://schemas.xmlsoap.org/soap/envelope/"),
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
    }
}
