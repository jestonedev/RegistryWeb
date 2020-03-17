using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace RegistryWeb.Controllers
{
    public class AccountController : RegistryBaseController
    {
        private IConfiguration config;
        public AccountController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var server = "db01";
                var port = "3306";
                if (config.GetValue<bool>("IsNeorProfileSql"))
                {
                    server = "localhost";
                    port = "4040";
                }
                var connectionString =
                    "server=" + server + ";" +
                    "port=" + port + ";" +
                    "user=" + model.User + ";" +
                    "password=" + model.Password + ";" +
                    "database=" + config.GetValue<string>("Database") + ";";
                    //"convert zero datetime=true;";
                try 
                {
                    var conn = new MySqlConnection(connectionString);
                    conn.Open();
                    conn.Close();
                    Authenticate(model.User, connectionString);
                }
                catch (Exception ex)
                {
                    if (ex is MySqlException && ((MySqlException)ex).Number == 0)
                    {
                        TempData["Error"] = "Логин или пароль введены неверно!";
                    }
                    else
                    {
                        TempData["Error"] = "Ошибка соединения<br />" + ex.Message;
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        private async Task Authenticate(string userName, string connString)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName),
                new Claim("connString", connString, ClaimValueTypes.String)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}