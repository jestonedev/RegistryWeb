﻿using Microsoft.AspNetCore.Http;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.SecurityServices
{
    public class SecurityService
    {
        public AclUser User { get; set; }
        public List<AclPrivilege> Privileges { get; set; }
        public uint PrivelegesFlagValue { get; set; }

        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly RegistryContext registryContext;

        public SecurityService(RegistryContext registryContext, IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.registryContext = registryContext;
            User = GetUser();
            Privileges = GetUserPriveleges();
            var pr = Privileges.Select(p => p.PrivilegeMask).ToList();
            if (Privileges.Count() == 0)
                PrivelegesFlagValue = (uint)SecurityServices.Privileges.None;
            else
                PrivelegesFlagValue = Privileges.Select(p => p.PrivilegeMask).Aggregate((x, y) => x | y);
        }

        public bool HasPrivilege(Privileges privilege)
        {
            //return Privileges.Any(p => p.PrivilegeMask == (uint)privilege);
            return (PrivelegesFlagValue & (uint)privilege) == (uint)privilege;
        }

        private AclUser GetUser()
        {
            var userName = httpContextAccessor.HttpContext.User.Identity.Name.ToLowerInvariant();
            return registryContext.AclUsers.FirstOrDefault(u => u.UserName.ToLowerInvariant() == userName);
        }

        private List<AclPrivilege> GetUserPriveleges()
        {
            if (User == null)
                return new List<AclPrivilege>();
            var p1 = registryContext.AclUserPrivileges
                .Include(up => up.IdAclPrivilegeNavigation)
                .Where(up => up.IdUser == User.IdUser)
                .Select(up => up.IdAclPrivilegeNavigation);
            var p2 = from userRole in registryContext.AclUserRoles.Where(ur => ur.IdUser == User.IdUser)
                     join rolePrivilage in registryContext.AclRolePrivileges.Include(rp => rp.IdAclPrivilegeNavigation)
                        on userRole.IdRole equals rolePrivilage.IdRole
                     select rolePrivilage.IdAclPrivilegeNavigation;
            return p1.Union(p2).ToList();
        }
    }
}