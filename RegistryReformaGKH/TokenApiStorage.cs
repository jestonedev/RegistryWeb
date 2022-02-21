using System;

namespace RegistryReformaGKH
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
