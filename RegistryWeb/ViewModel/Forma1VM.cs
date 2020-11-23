using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class Forma1VM : ListVM<BuildingsFilter>
    {
        public IQueryable<Building> Buildings { get; set; }
    }
}
