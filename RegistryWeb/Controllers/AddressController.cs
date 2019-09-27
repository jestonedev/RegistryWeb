using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    [Authorize]
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
                .AsNoTracking()
                .Where(s => s.StreetLong.Contains(text) || s.StreetName.Contains(text))
                .Select(s => new { s.IdStreet, s.StreetName });
            return Json(model);
        }

        [HttpPost]
        public JsonResult AutocompleteFilterOptionsAddress(string text)
        {
            var addressWords = text.Trim().Split(' ');
            var street = addressWords[0].ToLowerInvariant();
            var addressType = AddressTypes.None;
            IEnumerable<Tuple<string, string>> autocompletePairs = null;
            if (addressWords.Length == 1)
            {
                addressType = AddressTypes.Street;
                autocompletePairs = registryContext.KladrStreets
                    .AsNoTracking()
                    .Where(s => s.StreetLong.ToLowerInvariant().Contains(street) ||
                                s.StreetName.ToLowerInvariant().Contains(street))
                    .Select(s => new Tuple<string, string>(s.IdStreet, s.StreetName));
            }
            else
            {
                var house = addressWords[1].ToLowerInvariant();
                if (addressWords.Length == 2)
                {
                    addressType = AddressTypes.Building;
                    autocompletePairs = registryContext.Buildings
                        .Include(b => b.IdStreetNavigation)
                        .AsNoTracking()
                        .Where(b => (b.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(street) ||
                                    b.IdStreetNavigation.StreetName.ToLowerInvariant().Contains(street))
                                    && b.House.ToLowerInvariant().Contains(house))
                        .Select(b => new Tuple<string, string>(
                            b.IdBuilding.ToString(),
                            string.Concat(b.IdStreetNavigation.StreetName, ", д.", b.House)));
                }
                else
                {
                    var premiseNum = addressWords[2].ToLowerInvariant();
                    if (addressWords.Length == 3)
                    {
                        addressType = AddressTypes.Premise;
                        autocompletePairs = registryContext.Premises
                            .Include(p => p.IdPremisesType)
                            .Include(p => p.IdBuildingNavigation)
                                .ThenInclude(b => b.IdStreetNavigation)
                            .AsNoTracking()
                            .Where(p => (p.IdBuildingNavigation.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(street) ||
                                        p.IdBuildingNavigation.IdStreetNavigation.StreetName.ToLowerInvariant().Contains(street))
                                        && p.IdBuildingNavigation.House.ToLowerInvariant().Contains(house)
                                        && p.PremisesNum.ToLowerInvariant().Contains(premiseNum))
                            .Select(p => new Tuple<string, string>(
                                p.IdPremises.ToString(),
                                string.Concat(p.IdBuildingNavigation.IdStreetNavigation.StreetName, ", д.", p.IdBuildingNavigation.House,
                                    ", ", p.IdPremisesTypeNavigation.PremisesTypeShort, p.PremisesNum)));
                    }
                    else if (addressWords.Length == 4)
                    {
                        addressType = AddressTypes.SubPremise;
                        var subPremisesNum = addressWords[3].ToLowerInvariant();
                        autocompletePairs = registryContext.SubPremises
                            .Include(sp => sp.IdPremisesNavigation)
                                .ThenInclude(p => p.IdPremisesType)
                            .Include(sp => sp.IdPremisesNavigation)
                                .ThenInclude(p => p.IdBuildingNavigation)
                                    .ThenInclude(b => b.IdStreetNavigation)
                            .AsNoTracking()
                            .Where(sp => (sp.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(street) ||
                                        sp.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName.ToLowerInvariant().Contains(street))
                                        && sp.IdPremisesNavigation.IdBuildingNavigation.House.ToLowerInvariant().Contains(house)
                                        && sp.IdPremisesNavigation.PremisesNum.ToLowerInvariant().Contains(premiseNum)
                                        && sp.SubPremisesNum.ToLowerInvariant().Contains(subPremisesNum))
                            .Select(sp => new Tuple<string, string>(
                                sp.IdSubPremises.ToString(),
                                string.Concat(sp.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", д.", 
                                    sp.IdPremisesNavigation.IdBuildingNavigation.House, ", ",
                                    sp.IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort, 
                                    sp.IdPremisesNavigation.PremisesNum, ", к.", sp.SubPremisesNum)));
                    }
                }
            }
            return Json(new { idAddressType = (int)addressType, autocompletePairs });
        }

        [HttpPost]
        public IActionResult GetBuildingsSelectList(string idStreet)
        {            
            var buildings = registryContext.Buildings
                .AsNoTracking()
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
            var premisesTypes = registryContext.Premises
                .Include(p => p.IdPremisesTypeNavigation)
                .AsNoTracking()
                .Where(p => p.IdBuilding == idBuilding)
                .Select(p => p.IdPremisesTypeNavigation)
                .Distinct();
            StringBuilder str = new StringBuilder();
            foreach (var pt in premisesTypes)
                str.Append("<option value=\"" + pt.IdPremisesType + "\">" + pt.PremisesTypeName + "</option>");
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
                .AsNoTracking()
                .Where(p => p.IdBuilding == idBuilding && p.IdPremisesType == idPremisesType);
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
                .AsNoTracking()
                .Where(sp => sp.IdPremises == idPremise);
            StringBuilder str = new StringBuilder();
            foreach (var sp in subPremises)
                str.Append("<option value=\"" + sp.IdSubPremises + "\">" + sp.SubPremisesNum + "</option>");
            return Content(str.ToString());
        }
    }
}