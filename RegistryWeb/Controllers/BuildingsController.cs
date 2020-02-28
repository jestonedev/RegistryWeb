using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class BuildingsController : ListController<BuildingsDataService>
    {
        public BuildingsController(BuildingsDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
        }

        public IActionResult Index(BuildingsVM viewModel)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            ViewBag.ObjectStates = dataService.ObjectStates;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        public IActionResult BuildingReports()
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (HttpContext.Session.Keys.Contains("idBuildings"))
            {
                var ids = HttpContext.Session.Get<List<int>>("idBuildings");
                if (ids.Any())
                {
                    var buildings = dataService.GetBuildings(ids);
                    return View("BuildingReports", buildings);
                }
            }
            return View("BuildingReports", new List<Building>());
        }

        //[HttpPost]
        //public IActionResult BuildingReports()
        //{
        //    if (!securityService.HasPrivilege(Privileges.RegistryRead))
        //        return View("NotAccess");
            
        //    return View("BuildingReports");
        //}

        [HttpPost]
        public void SessionIdBuildings(int idBuilding, bool isCheck)
        {
            List<int> ids;
            if (HttpContext.Session.Keys.Contains("idBuildings"))
            {
                ids = HttpContext.Session.Get<List<int>>("idBuildings");
            }
            else
            {
                ids = new List<int>();
            }
            if (isCheck)
            {
                ids.Add(idBuilding);
            }
            else if(ids.Any())
            {
                ids.Remove(idBuilding);
            }
            HttpContext.Session.Set("idBuildings", ids);
        }

        public IActionResult SessionIdBuildingsClear()
        {
            HttpContext.Session.Remove("idBuildings");
            return BuildingReports();
        }

        public IActionResult SessionIdBuildingRemove(int idBuilding)
        {
            var ids = HttpContext.Session.Get<List<int>>("idBuildings");
            ids.Remove(idBuilding);
            HttpContext.Session.Set("idBuildings", ids);
            return BuildingReports();
        }

        public IActionResult Details(int? idBuilding)
        {
            if (idBuilding == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            var building = dataService.GetBuilding(idBuilding.Value);
            if (building == null)
                return NotFound();
            return GetBuildingView(building);
        }

        public IActionResult GetBuildingView(Building building, [CallerMemberName]string action = "")
        {
            ViewBag.Action = action;
            ViewBag.ObjectStates = dataService.ObjectStates;
            ViewBag.StructureTypes = dataService.StructureTypes;
            ViewBag.KladrStreets = dataService.KladrStreets;
            ViewBag.HeatingTypes = dataService.HeatingTypes;
            return View("Building", building);
        }

        public IActionResult Error(string message = "")
        {
            ViewData["TextError"] = message;
            ViewData["Controller"] = "Buildings";
            return View("Error");
        }
    }
}
