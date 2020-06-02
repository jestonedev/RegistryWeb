using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class OwnershipRightsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;

        public OwnershipRightsController(SecurityService securityService, RegistryContext registryContext)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
        }

        [HttpPost]
        public int DeleteOwnershipRight(int? idOwnershipRight)
        {
            if (idOwnershipRight == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return -2;
            try
            {
                var ownershipRight = registryContext.OwnershipRights
                    .FirstOrDefault(op => op.IdOwnershipRight == idOwnershipRight);
                ownershipRight.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetOwnershipRight(int? idOwnershipRight)
        {
            if (idOwnershipRight == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var ownershipRight = registryContext.OwnershipRights
                .Include(or => or.OwnershipRightTypeNavigation)
                .FirstOrDefault(op => op.IdOwnershipRight == idOwnershipRight);
            return Json(new {
                number = ownershipRight.Number ?? "",
                date = ownershipRight.Date.ToString("yyyy-MM-dd"),
                description = ownershipRight.Description ?? "",
                idOwnershipRightType = ownershipRight.IdOwnershipRightType,
                resettlePlanDate = ownershipRight.ResettlePlanDate.HasValue ? ownershipRight.ResettlePlanDate.Value.ToString("yyyy-MM-dd") : "",
                demolishPlanDate = ownershipRight.DemolishPlanDate.HasValue ? ownershipRight.DemolishPlanDate.Value.ToString("yyyy-MM-dd") : "",
            });
        }

        [HttpPost]
        public int YesOwnershipRight(OwnershipRight owr, Address address)
        {
            if (owr == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return -2;
            //Создать
            if (owr.IdOwnershipRight == 0)
            {
                registryContext.OwnershipRights.Add(owr);
                registryContext.SaveChanges();
                var id = 0;
                if (address == null)
                    return -3;
                if (!int.TryParse(address.Id, out id))
                    return -4;
                if (address.AddressType == AddressTypes.Building)
                {
                    var oba = new OwnershipBuildingAssoc()
                    {
                        IdBuilding = id,
                        IdOwnershipRight = owr.IdOwnershipRight
                    };
                    registryContext.OwnershipBuildingsAssoc.Add(oba);
                    registryContext.SaveChanges();
                }
                if (address.AddressType == AddressTypes.Premise)
                {
                    var opa = new OwnershipPremiseAssoc()
                    {
                        IdPremises = id,
                        IdOwnershipRight = owr.IdOwnershipRight
                    };
                    registryContext.OwnershipPremisesAssoc.Add(opa);
                    registryContext.SaveChanges();
                }
                return owr.IdOwnershipRight;
            }
            //Обновить            
            registryContext.OwnershipRights.Update(owr);
            registryContext.SaveChanges();
            return 0;
        }

        [HttpPost]
        public IActionResult AddOwnershipRight(AddressTypes addressType, string action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-1);
            var ownershipRightTypes = registryContext.OwnershipRightTypes.AsNoTracking();
            var tr = new StringBuilder();
            tr.Append("<tr class=\"ownership-right\" data-idownershipright=\"" + Guid.NewGuid() + "\">");
            if(addressType == AddressTypes.Premise)
            {
                tr.Append("<td class=\"align-middle\">Помещение</td>");
            }
            tr.Append("<td class=\"align-middle\"><input type=\"text\" class=\"form-control field-ownership-right\"></td>");
            tr.Append("<td class=\"align-middle\"><input type=\"date\" class=\"form-control field-ownership-right\"></td>");
            tr.Append("<td class=\"align-middle\"><input type=\"text\" class=\"form-control field-ownership-right\"></td>");
            //Формирование селекта для ownershipRightTypes
            var tdIdOwnershipRightType = new StringBuilder();
            tdIdOwnershipRightType.Append("<td class=\"align-middle\">");
            tdIdOwnershipRightType.Append("<select class=\"form-control field-ownership-right\">");
            foreach (var owrt in ownershipRightTypes)
            {
                tdIdOwnershipRightType.Append("<option value=\"" + owrt.IdOwnershipRightType + "\">" + owrt.OwnershipRightTypeName + "</option>");
            }
            tdIdOwnershipRightType.Append("</select>");
            tdIdOwnershipRightType.Append("</td>");
            tr.Append(tdIdOwnershipRightType);
            tr.Append("<td class=\"align-middle\"><input type=\"date\" class=\"form-control field-ownership-right\"></td>");
            tr.Append("<td class=\"align-middle\"><input type=\"date\" class=\"form-control field-ownership-right\"></td>");
            //Панели
            tr.Append("<td class=\"align-middle\">");
            tr.Append("<div class=\"btn-group yes-no-panel\" role=\"group\" aria-label=\"Панель подтверждения\">");
            tr.Append("<a class=\"btn btn-danger oi oi-x\" title=\"Нет\" aria-label=\"Нет\"></a>");
            if (action == "Edit")
            {
                tr.Append("<a class=\"btn btn-success oi oi-check\" title=\"Да\" aria-label=\"Да\"></a>");
                tr.Append("</div>");
                tr.Append("<div class=\"btn-group edit-del-panel\" role=\"group\" aria-label=\"Панель реадктирования\" style=\"display: none;\">");
                tr.Append("<a class=\"btn btn-primary oi oi-pencil\" title=\"Редактировать\" aria-label=\"Редактировать\"></a>");
                tr.Append("<a class=\"btn btn-danger oi oi-x delete\" title=\"Удалить\" aria-label=\"Удалить\"></a>");
            }
            tr.Append("</div>");
            tr.Append("</td>");
            tr.Append("</tr>");
            return Content(tr.ToString());
        }
    }
}