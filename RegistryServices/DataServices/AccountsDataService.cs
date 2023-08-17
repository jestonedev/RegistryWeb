using RegistryDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities;
using RegistryDb.Models.SqlViews;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.Enums;
using RegistryServices.Models;
using RegistryDb.Models.Entities.Common;
using RegistryServices.Models.KumiAccounts;
using RegistryWeb.DataHelpers;
using RegistryServices.Classes;
using RegistryDb.Models.Entities.Payments;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

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
