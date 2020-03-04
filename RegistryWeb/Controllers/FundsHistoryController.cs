using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;

namespace RegistryWeb.Controllers
{
    public class FundsHistoryController : RegistryBaseController
    {
        private readonly FundsHistoryDataService dataService;

        public FundsHistoryController(FundsHistoryDataService dataService)
        {
            this.dataService = dataService;
        }

        public IActionResult Index(int idPremises)
        {
            return View("FundsHistory", dataService.GetViewModel(idPremises));
        }
    }
}