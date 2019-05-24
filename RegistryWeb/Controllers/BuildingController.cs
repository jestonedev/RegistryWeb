using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Collections.Generic;
using RegistryWeb.DataServices;

namespace RegistryWeb.Controllers
{
    public class BuildingController : Controller
    {
        private BuildingDataService dataService;

        public BuildingController(BuildingDataService dataService)
        {
            this.dataService = dataService;
        }

        public IActionResult Details(int idBuilding)
        {
            return View(dataService.GetViewModel(idBuilding));
        }

        public IActionResult Edit(int idBuilding)
        {
            return View(dataService.GetViewModel(idBuilding));
        }

        public IActionResult Delete()
        {
            return View();
        }
    }
}
