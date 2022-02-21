using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataServices;
using RegistryWeb.Enums;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class AddressController : RegistryBaseController
    {
        private readonly AddressesDataService addressesDataService;

        public AddressController(AddressesDataService addressesDataService)
        {
            this.addressesDataService = addressesDataService;
        }

        public JsonResult AutocompleteStreet(string text)
        {
            var model = addressesDataService.GetStreetsByText(text).Select(s => new { s.IdStreet, s.StreetName });
            return Json(model);
        }

        public JsonResult AutocompleteFilterOptionsAddress(string text, AddressTypes addressTypes = AddressTypes.Premise)
        {
            var addresses = addressesDataService.GetAddressesByText(text, addressTypes).GroupBy(a => a.AddressType);
            return Json(new { addressType = addresses.FirstOrDefault()?.Key.ToString(),
                autocompletePairs = addresses.FirstOrDefault()?.Select(a => new Tuple<string, string>(
                a.Id,
                a.Text
            )) });
        }

        public JsonResult AutocompleteFilterOptionsAddressAlt(string text, AddressTypes addressTypes = AddressTypes.Premise)
        {
            var addresses = addressesDataService.GetAddressesByText(text, addressTypes).GroupBy(a => a.AddressType);
            return Json(new
            {
                addressType = addresses.FirstOrDefault()?.Key.ToString(),
                addresses = addresses.FirstOrDefault()?.Select(a => a.Text).Distinct()
            });
        }

        public IActionResult GetBuildingsSelectList(string idStreet)
        {
            var buildings = addressesDataService.GetBuildings(idStreet);
            StringBuilder str = new StringBuilder();
            foreach(var b in buildings)
                str.Append("<option value=\"" + b.IdBuilding + "\">" + b.House + "</option>");
            return Content(str.ToString());
        }

        public IActionResult GetPremisesTypeSelectList(int idBuilding)
        {
            if (idBuilding == 0)
                return Content("");

            var premisesTypes = addressesDataService.GetPremiseTypesByBuilding(idBuilding);
            StringBuilder str = new StringBuilder();
            foreach (var pt in premisesTypes)
                str.Append("<option value=\"" + pt.IdPremisesType + "\">" + pt.PremisesTypeName + "</option>");
            return Content(str.ToString());
        }

        public JsonResult GetPremisesTypeAsNumText(int idPremisesType)
        {
            if (idPremisesType == 0)
                return Json(new { premisesTypeAsNum = "Номер" });
            var premisesTypeAsNum = addressesDataService.PremisesTypes
                .First(pt => pt.IdPremisesType == idPremisesType).PremisesTypeAsNum;
            return Json(new { premisesTypeAsNum });
        }

        public IActionResult GetPremisesNumSelectList(int idBuilding, int idPremisesType)
        {
            if (idPremisesType == 0 || idBuilding == 0)
                return Content("");
            var premises = addressesDataService.GetPremisesByBuildingAndType(idBuilding, idPremisesType);
            StringBuilder str = new StringBuilder();
            foreach (var p in premises)
                str.Append("<option value=\"" + p.IdPremises + "\">" + p.PremisesNum + "</option>");
            return Content(str.ToString());
        }

        public IActionResult GetSubPremisesNumSelectList(int idPremise)
        {
            if (idPremise == 0)
                return Content("");
            var subPremises = addressesDataService.GetSubPremisesByPremisee(idPremise);
            StringBuilder str = new StringBuilder();
            foreach (var sp in subPremises)
                str.Append("<option value=\"" + sp.IdSubPremises + "\">" + sp.SubPremisesNum + "</option>");
            return Content(str.ToString());
        }

        public JsonResult GetKladrStreets(string idRegion)
        {
            var streets = addressesDataService.GetKladrStreets(idRegion);
            return Json(streets);
        }

        public JsonResult GetBuilding(string idStreet)
        {
            var buildings = addressesDataService.GetBuildings(idStreet);
            return Json(buildings);
        }

        public JsonResult GetPremises(int? idBuilding)
        {
            var premises = addressesDataService.GetPremises(idBuilding);
            return Json(premises);
        }

        public JsonResult GetSubPremises(int? idPremise)
        {
            var subPremises = addressesDataService.GetSubPremises(idPremise);
            return Json(subPremises);
        }

        public JsonResult GetAddressRegistryModal(PartsAddress parts)
        {
            var addressList = addressesDataService.GetAddressesFromHisParts(parts);
            if (addressList == null || addressList.Count() == 0)
                return Json(-1);
            if (addressList.Count() > 1)
                return Json(-2);
            return Json(addressList[0]);
        }
    }
}