using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;

namespace RegistryWeb.Controllers
{
    public class ListController<LDS> : Controller where LDS : IListDataService
    {
        protected readonly LDS dataService;

        public ListController(LDS dataService)
        {
            this.dataService = dataService;
        }
    }
}