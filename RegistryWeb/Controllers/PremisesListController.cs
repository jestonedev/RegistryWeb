using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    public class PremisesListController : ListController<PremisesListDataService>
    {
        public PremisesListController(PremisesListDataService dataService) : base(dataService)
        {
        }

        public IActionResult Index(PremisesListVM viewModel)
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
