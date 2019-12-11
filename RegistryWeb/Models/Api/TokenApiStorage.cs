using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Api
{
    public static class TokenApiStorage
    {
        public static string User { get; set; }
        public static string Password { get; set; }

        public static DateTime DateCreateTokenApi { get; private set; }

        private static string sessionGuid;
        public static string SessionGuid
        {
            get { return sessionGuid; }
            set
            {
                DateCreateTokenApi = DateTime.Now;
                sessionGuid = value;
            }
        }
    }
}
