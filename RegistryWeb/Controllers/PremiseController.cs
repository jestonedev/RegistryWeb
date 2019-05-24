using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using RegistryWeb.DataServices;

namespace RegistryWeb.Controllers
{
    public class PremiseController : Controller
    {
        private PremiseDataService dataService;

        public PremiseController(PremiseDataService dataService)
        {
            this.dataService = dataService;
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Index(int idPremises)
        {
            return View(dataService.GetViewModel(idPremises));
        }

        public IActionResult Edit(int idPremises)
        {
            return View(dataService.GetViewModel(idPremises));
        }

        public IActionResult Delete(int idPremises)
        {
            return View();
        }
    }
}
