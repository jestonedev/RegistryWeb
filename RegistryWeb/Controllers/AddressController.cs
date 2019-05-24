using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    public class AddressController : Controller
    {
        private RegistryContext registryContext;

        public AddressController(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        [HttpPost]
        public JsonResult AutocompleteStreet(string text)
        {
            var model = registryContext.KladrStreets
                .Where(s => s.StreetLong.Contains(text) || s.StreetName.Contains(text))
                .Select(s => new { s.IdStreet, s.StreetName });
            return Json(model);
        }

        [HttpPost]
        public IActionResult GetBuildingsSelectList(string idStreet)
        {            
            var buildings = registryContext.Buildings
                .Where(b => b.IdStreet == idStreet);
            StringBuilder str = new StringBuilder();
            foreach(var b in buildings)
                str.Append("<option value=\"" + b.IdBuilding + "\">" + b.House + "</option>");
            return Content(str.ToString());
        }

        [HttpPost]
        public IActionResult GetPremisesTypeSelectList(int idBuilding)
        {
            if (idBuilding == 0)
                return Content("");
            var idPremisesTypes = registryContext.Premises
                .Where(p => p.IdBuilding == idBuilding)
                .Select(p => p.IdPremisesType)
                .Distinct();
            var premisesTypes = registryContext.PremisesTypes.
                Where(pt => idPremisesTypes.Contains(pt.IdPremisesType));
            StringBuilder str = new StringBuilder();
            foreach (var pt in premisesTypes)
                str.Append("<option value=\"" + pt.IdPremisesType + "\">" + pt.PremisesType + "</option>");
            return Content(str.ToString());
        }

        [HttpPost]
        public JsonResult GetPremisesTypeAsNumText(int idPremisesType)
        {
            if (idPremisesType == 0)
                return Json(new { premisesTypeAsNum = "Номер" });
            var premisesTypeAsNum = registryContext.PremisesTypes
                .First(pt => pt.IdPremisesType == idPremisesType).PremisesTypeAsNum;
            return Json(new { premisesTypeAsNum });
        }

        [HttpPost]
        public IActionResult GetPremisesNumSelectList(int idBuilding, int idPremisesType)
        {
            if (idPremisesType == 0 || idBuilding == 0)
                return Content("");
            var premises = registryContext.Premises
                .Where(p => p.IdBuilding == idBuilding && p.IdPremisesType == idPremisesType).ToList();
            StringBuilder str = new StringBuilder();
            foreach (var p in premises)
                str.Append("<option value=\"" + p.IdPremises + "\">" + p.PremisesNum + "</option>");
            return Content(str.ToString());
        }

        [HttpPost]
        public IActionResult GetSubPremisesNumSelectList(int idPremise)
        {
            if (idPremise == 0)
                return Content("");
            var subPremises = registryContext.SubPremises
                .Where(sp => sp.IdPremises == idPremise).ToList();
            StringBuilder str = new StringBuilder();
            foreach (var sp in subPremises)
                str.Append("<option value=\"" + sp.IdSubPremises + "\">" + sp.SubPremisesNum + "</option>");
            return Content(str.ToString());
        }
    }
}