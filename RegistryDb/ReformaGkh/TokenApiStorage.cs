using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public class TokenApiStorage
    {
        public TokenApiStorage(){}

        public string User { get; set; }
        public string Password { get; set; }
        public DateTime DateCreateTokenApi { get; private set; }

        private string sessionGuid;
        public string SessionGuid
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
