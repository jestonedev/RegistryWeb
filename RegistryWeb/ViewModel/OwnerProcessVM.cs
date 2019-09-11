using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Linq;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessVM
    {
        public OwnerProcess OwnerProcess { get; set; }
        public IQueryable<GroupChangeLog> Log { get; set; }
    }
}