using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Linq;

namespace RegistryPaymentCalculator
{
    internal class Configuration
    {
        public static string DbName 
        {
            get
            {
                return ConfigurationManager.AppSettings["dbName"];
            }
        }

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["registry"].ConnectionString;
            }
        }

        public static string SmtpHost
        {
            get
            {
                return ConfigurationManager.AppSettings["smtpHost"];
            }
        }

        public static int SmtpPort
        {
            get
            {
                if (!int.TryParse(ConfigurationManager.AppSettings["smtpPort"], out int port))
                {
                    port = 25;
                }
                return port;
            }
        }

        public static string SmtpFrom
        {
            get
            {
                return ConfigurationManager.AppSettings["smtpFrom"];
            }
        }

        public static List<string> SmtpErrorTo
        {
            get
            {
                var toStr = ConfigurationManager.AppSettings["smtpErrorTo"];
                if (string.IsNullOrWhiteSpace(toStr)) return new List<string>();
                return toStr.Split(',').Select(r => r.Trim()).ToList();
            }
        }
        public static List<string> SmtpSuccessTo
        {
            get
            {
                var toStr = ConfigurationManager.AppSettings["smtpSuccessTo"];
                if (string.IsNullOrWhiteSpace(toStr)) return new List<string>();
                return toStr.Split(',').Select(r => r.Trim()).ToList();
            }
        }

    }
}
