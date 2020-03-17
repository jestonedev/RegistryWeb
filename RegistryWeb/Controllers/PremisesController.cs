using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using System.Runtime.CompilerServices;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PremisesController : ListController<PremisesDataService>
    {
        public PremisesController(PremisesDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
        }

        public IActionResult Index(PremisesListVM viewModel)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        public IActionResult Details(int? idPremises)
        {
            if (idPremises == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();
            return GetBuildingView(premise);
        }

        public IActionResult GetBuildingView(Premise premise, [CallerMemberName]string action = "")
        {
            ViewBag.Action = action;
            ViewBag.ObjectStates = dataService.ObjectStates;
            ViewBag.StructureTypes = dataService.StructureTypes;
            ViewBag.KladrStreets = dataService.KladrStreets;
            ViewBag.HeatingTypes = dataService.HeatingTypes;
            return View("Premise", premise);
        }/**/
    }
}
