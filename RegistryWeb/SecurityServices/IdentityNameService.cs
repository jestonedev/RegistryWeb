using Microsoft.AspNetCore.Http;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.SecurityServices
{
    public class IdentityNameService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public IdentityNameService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetUser() =>
            httpContextAccessor.HttpContext.User.Identity.Name.ToUpperInvariant();

    }
}
