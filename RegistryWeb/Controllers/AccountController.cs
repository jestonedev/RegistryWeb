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

namespace RegistryWeb.Controllers
{
    public class AccountController : Controller
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
        public IActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                
                var connectionString =
                    "Server=db01;" +
                    "UserId=" + model.User + ";" +
                    "Password=" + model.Password + ";" +
                    "Database=" + config.GetValue<string>("Database") + ";";
                try 
                {
                    var conn = new MySqlConnection(connectionString);
                    conn.Open();
                    conn.Close();
                }
                catch
                {
                    return Content("Ошибка соединения");
                }
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
}