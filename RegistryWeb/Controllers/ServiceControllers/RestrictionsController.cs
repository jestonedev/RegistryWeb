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
    public class RestrictionsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;

        public RestrictionsController(SecurityService securityService, RegistryContext registryContext)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
        }

        [HttpPost]
        public int DeleteRestriction(int? idRestriction)
        {
            if (idRestriction == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return -2;
            try
            {
                var restriction = registryContext.Restrictions
                    .FirstOrDefault(op => op.IdRestriction == idRestriction);
                restriction.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetRestriction(int? idRestriction)
        {
            if (idRestriction == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var restriction = registryContext.Restrictions
                .Include(or => or.RestrictionTypeNavigation)
                .FirstOrDefault(op => op.IdRestriction == idRestriction);
            return Json(new {
                number = restriction.Number,
                date = restriction.Date.ToString("yyyy-MM-dd"),
                description = restriction.Description,
                idRestrictionType = restriction.IdRestrictionType,
            });
        }

        [HttpPost]
        public int YesRestriction(Restriction restriction, Address address)
        {
            if (restriction == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return -2;
            //Создать
            if (restriction.IdRestriction == 0)
            {
                registryContext.Restrictions.Add(restriction);
                registryContext.SaveChanges();
                var id = 0;
                if (address == null)
                    return -3;
                if (!int.TryParse(address.Id, out id))
                    return -4;
                if (address.AddressType == AddressTypes.Building)
                {
                    var rba = new RestrictionBuildingAssoc()
                    {
                        IdBuilding = id,
                        IdRestriction = restriction.IdRestriction
                    };
                    registryContext.RestrictionBuildingsAssoc.Add(rba);
                    registryContext.SaveChanges();
                }
                if (address.AddressType == AddressTypes.Premise)
                {
                    var rpa = new RestrictionPremiseAssoc()
                    {
                        IdPremises = id,
                        IdRestriction = restriction.IdRestriction
                    };
                    registryContext.RestrictionPremisesAssoc.Add(rpa);
                    registryContext.SaveChanges();
                }
                return restriction.IdRestriction;
            }
            //Обновить            
            registryContext.Restrictions.Update(restriction);
            registryContext.SaveChanges();
            return 0;
        }

        [HttpPost]
        public IActionResult AddRestriction(AddressTypes addressType, string action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-1);
            var restrictionTypes = registryContext.RestrictionTypes.AsNoTracking();
            var tr = new StringBuilder();
            tr.Append("<tr class=\"restriction\" data-idrestriction=\"" + Guid.NewGuid() + "\">");
            if(addressType == AddressTypes.Premise)
            {
                tr.Append("<td class=\"align-middle\">Помещение</td>");
            }
            tr.Append("<td class=\"align-middle\"><input type=\"text\" class=\"form-control field-restriction\"></td>");
            tr.Append("<td class=\"align-middle\"><input type=\"date\" class=\"form-control field-restriction\"></td>");
            tr.Append("<td class=\"align-middle\"><input type=\"text\" class=\"form-control field-restriction\"></td>");
            //Формирование селекта для restrictionTypes
            var tdIdRestrictionType = new StringBuilder();
            tdIdRestrictionType.Append("<td class=\"align-middle\">");
            tdIdRestrictionType.Append("<select class=\"form-control field-restriction\">");
            foreach (var rt in restrictionTypes)
            {
                tdIdRestrictionType.Append("<option value=\"" + rt.IdRestrictionType + "\">" + rt.RestrictionTypeName + "</option>");
            }
            tdIdRestrictionType.Append("</select>");
            tdIdRestrictionType.Append("</td>");
            tr.Append(tdIdRestrictionType);
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