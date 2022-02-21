using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryDb.Interfaces;

namespace RegistryWeb.DataHelpers
{
    public class DbConnectionSettings : IDbConnectionSettings
    {
        private string nameDatebase;
        private string connString;

        public DbConnectionSettings(IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            nameDatebase = config.GetValue<string>("Database");
        }

        public string GetConnectionString()
        {
            return connString;
        }

        public string GetDbName()
        {
            return nameDatebase;
        }
    }
}
