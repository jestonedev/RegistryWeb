using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;
using MySql.Data.MySqlClient;

namespace RegistryWeb.SecurityServices
{
    public class CookieUserWithConnectionStringHandler : AuthorizationHandler<CookieUserWithConnectionStringRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CookieUserWithConnectionStringRequirement requirement)
        {
            if (!(context.User.Identity is WindowsIdentity))
            {
                var connString = context.User.FindFirstValue("connString");
                try
                {
                    var connPersonal = new MySqlConnection(connString);
                    connPersonal.Open();
                    connPersonal.Close();
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                } catch(Exception)
                {

                }
            }
            context.Fail();
            return Task.CompletedTask;
        }
    }
}