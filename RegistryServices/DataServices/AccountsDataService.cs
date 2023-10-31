using System;
using RegistryWeb.DataHelpers;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace RegistryWeb.DataServices
{
    public class AccountsDataService
    {
        private readonly IConfiguration config;

        public AccountsDataService(IConfiguration config)
        {
            this.config = config;
        }

        public string ConfigureConnectionString(string userName)
        {
            var connectionStringTemplate = "server=" + config.GetValue<string>("Server") + ";" +
               "port=" + config.GetValue<string>("Port") + ";" +
               "user={0};password={1};" +
               "database=" + config.GetValue<string>("Database") + "; convert zero datetime=True";

            var connectionString = string.Format(connectionStringTemplate, "registry", "registry");

            try
            {
                var conn = new MySqlConnection(connectionString);
                conn.Open();

                var query = new MySqlCommand("SELECT password FROM acl_users WHERE LOWER(user_name) = LOWER(@userName)", conn);
                query.Parameters.AddWithValue("@userName", userName);
                var password = (string)query.ExecuteScalar();
                conn.Close();
                var passwordBlank = AccountHelper.DecryptPassword(password);

                connectionString = string.Format(connectionStringTemplate, userName, passwordBlank);

                var connPersonal = new MySqlConnection(connectionString);
                connPersonal.Open();
                connPersonal.Close();
                
                return connectionString;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
