using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    public class BuildingsListController : ListController<BuildingsListDataService>
    {
        public BuildingsListController(BuildingsListDataService dataService) : base(dataService)
        {
        }

        public IActionResult Index(BuildingsListVM viewModel)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }
    }
}
