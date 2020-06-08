using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class BuildingDemolitionInfoController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;

        public BuildingDemolitionInfoController(SecurityService securityService, RegistryContext registryContext)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
        }

        [HttpPost]
        public IActionResult GetBuildingDemolitionInfo(int? idBuilding)
        {
            if (idBuilding == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var demolishPlanDate = registryContext.Buildings
                .SingleOrDefault(b => b.IdBuilding == idBuilding)
                .DemolishedPlanDate;
            var buildingDemolitionActFiles = registryContext.BuildingDemolitionActFiles
                .Where(b => b.IdBuilding == idBuilding)
                .AsNoTracking()
                .ToList();
            var model = new BuildingDemolitionInfoVM()
            {
                BuildingDemolitionActFiles = buildingDemolitionActFiles,
                DemolishPlanDate = demolishPlanDate
            };
            return Json(model);
        }

        [HttpPost]
        public IActionResult SaveBuildingDemolitionInfo(int? idBuilding, BuildingDemolitionInfoVM buildingDemolitionInfoVM)
        {
            if (buildingDemolitionInfoVM == null || idBuilding == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            try
            {
                registryContext.Buildings.SingleOrDefault(b => b.IdBuilding == idBuilding).DemolishedPlanDate = buildingDemolitionInfoVM.DemolishPlanDate;
                var oldActFiles = registryContext.BuildingDemolitionActFiles.
                    Where(af => af.IdBuilding == idBuilding)
                    .AsNoTracking();
                var newActFiles = buildingDemolitionInfoVM.BuildingDemolitionActFiles ?? new List<BuildingDemolitionActFile>();
                foreach (var oldFile in oldActFiles)
                {
                    if (newActFiles.Select(af => af.Id).Contains(oldFile.Id) == false)
                    {
                        registryContext.Entry(oldFile).Property(p => p.Deleted).IsModified = true;
                        oldFile.Deleted = 1;
                        newActFiles.Add(oldFile);
                    }
                }
                registryContext.BuildingDemolitionActFiles.UpdateRange(newActFiles);
                registryContext.SaveChanges();
                return Json(1);
            }
            catch
            {
                return Json(-3);
            }
        }

        [HttpPost]
        public IActionResult AddBuildingDemolitionActFile()
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-1);
            var actTypeDocuments = registryContext.ActTypeDocuments
                .Where(atd => atd.ActFileType == ActFileTypes.BuildingDemolitionActFile.ToString())
                .AsNoTracking();
            var tr = new StringBuilder();
            tr.Append("<tr class=\"ownership-right\" data-idownershipright=\"" + Guid.NewGuid() + "\">");
            tr.Append("<td class=\"align-middle\"><input type=\"text\" class=\"form-control field-ownership-right\"></td>");
            tr.Append("<td class=\"align-middle\"><input type=\"date\" class=\"form-control field-ownership-right\"></td>");
            tr.Append("<td class=\"align-middle\"><input type=\"text\" class=\"form-control field-ownership-right\"></td>");
            //Формирование селекта для ActTypeDocuments
            var tdIdOwnershipRightType = new StringBuilder();
            tdIdOwnershipRightType.Append("<td class=\"align-middle\">");
            tdIdOwnershipRightType.Append("<select class=\"form-control field-ownership-right\">");
            foreach (var type in actTypeDocuments)
            {
                tdIdOwnershipRightType.Append("<option value=\"" + type.Id + "\">" + type.Name + "</option>");
            }
            tdIdOwnershipRightType.Append("</select>");
            tdIdOwnershipRightType.Append("</td>");
            tr.Append(tdIdOwnershipRightType);
            tr.Append("<td class=\"align-middle\"><input type=\"date\" class=\"form-control field-ownership-right\"></td>");
            tr.Append("<td class=\"align-middle\"><input type=\"date\" class=\"form-control field-ownership-right\"></td>");
            //Панели
            tr.Append("<td class=\"align-middle\">");
            tr.Append("<a class=\"btn btn-danger oi oi-x delete\" title=\"Удалить\" aria-label=\"Удалить\"></a>");
            tr.Append("</td>");
            tr.Append("</tr>");
            return Content(tr.ToString());
        }
    }
}