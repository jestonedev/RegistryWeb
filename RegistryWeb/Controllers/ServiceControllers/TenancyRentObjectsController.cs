using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.Enums;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class TenancyRentObjectsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public TenancyRentObjectsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteRentObject(int? idProcess, int? idObject, AddressTypes addressType)
        {
            if (idObject == null || idProcess == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return -2;
            try
            {
                switch(addressType)
                {
                    case AddressTypes.Building:
                        var tenancyAssocBuilding = registryContext.TenancyBuildingsAssoc.FirstOrDefault(
                            r => r.IdBuilding == idObject && r.IdProcess == idProcess);
                        tenancyAssocBuilding.Deleted = 1;
                        break;
                    case AddressTypes.Premise:
                        var tenancyAssocPremises = registryContext.TenancyPremisesAssoc.FirstOrDefault(
                            r => r.IdPremise == idObject && r.IdProcess == idProcess);
                        tenancyAssocPremises.Deleted = 1;
                        break;
                    case AddressTypes.SubPremise:
                        var tenancyAssocSubPremises = registryContext.TenancySubPremisesAssoc.FirstOrDefault(
                            r => r.IdSubPremise == idObject && r.IdProcess == idProcess);
                        tenancyAssocSubPremises.Deleted = 1;
                        break;
                    default:
                        throw new Exception("Некорректный тип объекта найма");
                }
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult SaveRentObject(int? idProcess, int? idObject, double? rentArea, AddressTypes addressType, int? idObjectPrev, AddressTypes addressTypePrev)
        {
            if (idProcess == null || idObject == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(new { Error = -2 });
            if (idObject == idObjectPrev && addressType == addressTypePrev)
            {
                switch (addressType)
                {
                    case AddressTypes.Building:
                        var tenancyAssocBuilding = registryContext.TenancyBuildingsAssoc.FirstOrDefault(
                            r => r.IdBuilding == idObjectPrev && r.IdProcess == idProcess);
                        tenancyAssocBuilding.RentTotalArea = rentArea;
                        break;
                    case AddressTypes.Premise:
                        var tenancyAssocPremises = registryContext.TenancyPremisesAssoc.FirstOrDefault(
                            r => r.IdPremise == idObjectPrev && r.IdProcess == idProcess);
                        tenancyAssocPremises.RentTotalArea = rentArea;
                        break;
                    case AddressTypes.SubPremise:
                        var tenancyAssocSubPremises = registryContext.TenancySubPremisesAssoc.FirstOrDefault(
                            r => r.IdSubPremise == idObjectPrev && r.IdProcess == idProcess);
                        tenancyAssocSubPremises.RentTotalArea = rentArea;
                        break;
                    default:
                        throw new Exception("Некорректный тип объекта найма");
                }
                registryContext.SaveChanges();
                return Json(new { Error = 0 });
            }

            //Удалить старую привязку
            if (idObjectPrev != 0 && idObjectPrev != null)
            {
                switch (addressTypePrev)
                {
                    case AddressTypes.Building:
                        var tenancyAssocBuilding = registryContext.TenancyBuildingsAssoc.FirstOrDefault(
                            r => r.IdBuilding == idObjectPrev && r.IdProcess == idProcess);
                        tenancyAssocBuilding.Deleted = 1;
                        break;
                    case AddressTypes.Premise:
                        var tenancyAssocPremises = registryContext.TenancyPremisesAssoc.FirstOrDefault(
                            r => r.IdPremise == idObjectPrev && r.IdProcess == idProcess);
                        tenancyAssocPremises.Deleted = 1;
                        break;
                    case AddressTypes.SubPremise:
                        var tenancyAssocSubPremises = registryContext.TenancySubPremisesAssoc.FirstOrDefault(
                            r => r.IdSubPremise == idObjectPrev && r.IdProcess == idProcess);
                        tenancyAssocSubPremises.Deleted = 1;
                        break;
                    default:
                        throw new Exception("Некорректный тип объекта найма");
                }
            }
            //Добавить новую привязку       
            switch (addressType)
            {
                case AddressTypes.Building:
                    var tenancyAssocBuilding = registryContext.TenancyBuildingsAssoc.Add(new TenancyBuildingAssoc
                    {
                        IdProcess = idProcess.Value,
                        IdBuilding = idObject.Value,
                        RentTotalArea = rentArea
                    });
                    break;
                case AddressTypes.Premise:
                    var tenancyAssocPremises = registryContext.TenancyPremisesAssoc.Add(new TenancyPremiseAssoc
                    {
                        IdProcess = idProcess.Value,
                        IdPremise = idObject.Value,
                        RentTotalArea = rentArea
                    });
                    break;
                case AddressTypes.SubPremise:
                    var tenancyAssocSubPremises = registryContext.TenancySubPremisesAssoc.Add(new TenancySubPremiseAssoc
                    {
                        IdProcess = idProcess.Value,
                        IdSubPremise = idObject.Value,
                        RentTotalArea = rentArea
                    });
                    break;
                default:
                    throw new Exception("Некорректный тип объекта найма");
            }
            registryContext.SaveChanges();
            return Json(new { Error = 0 });
        }

        public JsonResult GetHouses(string idStreet)
        {
            IEnumerable<Building> buildings = registryContext.Buildings.Where(r => r.IdStreet == idStreet).OrderBy(r => r.House);
            return Json(buildings);
        }

        public JsonResult GetPremises(int? idBuilding)
        {
            IEnumerable<Premise> premises = registryContext.Premises.Where(r => r.IdBuilding == idBuilding).OrderBy(r => r.PremisesNum);
            return Json(premises);
        }

        public JsonResult GetSubPremises(int? idPremise)
        {
            IEnumerable<SubPremise> subPremises = registryContext.SubPremises.Where(r => r.IdPremises == idPremise).OrderBy(r => r.SubPremisesNum);
            return Json(subPremises);
        }
    }
}