using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using RegistryWeb.DataHelpers;
using RegistryWeb.DataServices;

namespace RegistryWeb.Controllers
{
    public class AccountController : RegistryBaseController
    {
        private readonly AccountsDataService dataService;

        public AccountController(AccountsDataService dataService)
        {
            this.dataService = dataService;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var userName = User.Identity.Name.ToUpper();
            if (userName == "PWR\\IGNATOV")
            {
                userName = "PWR\\IGNVV";
            }

            var connectionString = dataService.ConfigureConnectionString(userName);

            if (connectionString == null)
                return Error("Ошибка при соединении с базой данных. " +
                    "Возможно у вас нет прав доступа к данной программе. Нажмите кнопку \"Повторить\". " +
                    "В случае повтора ошибки обратитесь к администратору по телефону 349-671");
            else
            {
                await Authenticate(userName, connectionString);
                if (returnUrl == null)
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectPermanent(returnUrl);
            }
        }

        [AllowAnonymous]
        public IActionResult HashPassword(string password)
        {
            if (password == null)
                return Content("Не указан параметр password");
            return Content(AccountHelper.EncryptPassword(password));
        }

        public async Task<IActionResult> Refresh()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
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
    }
}