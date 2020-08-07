using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ReportServices;
using RegistryWeb.ViewModel;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class BuildingDemolitionInfoController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        ReportService reportService;

        public BuildingDemolitionInfoController(SecurityService securityService, RegistryContext registryContext, ReportService reportService)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.reportService = reportService;
        }

        [HttpPost]
        public JsonResult GetBuildingDemolitionInfo(int? idBuilding)
        {
            if (idBuilding == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var building = registryContext.Buildings
                .FirstOrDefault(b => b.IdBuilding == idBuilding);
            var demolishedPlanDate = building?.DemolishedPlanDate;
            var demolishedFactDate = building?.DemolishedFactDate;
            var dateOwnerEmergency = building?.DateOwnerEmergency;
            var demandForDemolishingDeliveryDate = building?.DemandForDemolishingDeliveryDate;
            var actTypeDocuments = registryContext.ActTypeDocuments
                .Where(atd => atd.ActFileType == ActFileTypes.BuildingDemolitionActFile.ToString())
                .Select(atd => new
                {
                    id = atd.Id,
                    name = atd.Name
                })
                .ToList();
            var buildingDemolitionActFiles = registryContext.BuildingDemolitionActFiles
                .Include(af => af.ActFile)
                .Where(b => b.IdBuilding == idBuilding)
                .OrderBy(b => b.Id).ToList()
                .Select(b => new
                {
                    id = b.Id,
                    idBuilding = b.IdBuilding,
                    idActFile = b.IdActFile,
                    idActTypeDocument = b.IdActTypeDocument,
                    number = b.Number,
                    date = b.Date.HasValue ? b.Date.Value.ToString("yyyy-MM-dd") : "",
                    name = b.Name,
                    originalNameActFile = b.ActFile == null ? "" : b.ActFile.OriginalName
                })
                .ToList();
            return Json(new
            {
                actTypeDocuments,
                buildingDemolitionActFiles,
                demolishedPlanDate = demolishedPlanDate.HasValue ? demolishedPlanDate.Value.ToString("yyyy-MM-dd") : "",
                demolishedFactDate = demolishedFactDate.HasValue ? demolishedFactDate.Value.ToString("yyyy-MM-dd") : "",
                dateOwnerEmergency = dateOwnerEmergency.HasValue ? dateOwnerEmergency.Value.ToString("yyyy-MM-dd") : "",
                demandForDemolishingDeliveryDate = demandForDemolishingDeliveryDate.HasValue ? demandForDemolishingDeliveryDate.Value.ToString("yyyy-MM-dd") : "",
                idBuilding = idBuilding.Value
            });
        }

        public IActionResult GetActFile(int? idFile)
        {
            if (idFile == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            try
            {
                var actFile = registryContext.ActFiles.SingleOrDefault(f => f.IdFile == idFile);
                var file = reportService.GetFileContentsFromRepository(actFile.FileName, ActFileTypes.BuildingDemolitionActFile);
                return File(file, actFile.MimeType, actFile.OriginalName);
            }
            catch
            {
                return Json(-3);
            }
        }

        [HttpPost]
        public IActionResult SaveBuildingDemolitionInfo(BuildingDemolitionInfoVM viewModel)
        {
            if (viewModel == null)
                return Json(-1);
            var r = Request;
            if (!securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo))
                return Json(-2);
            var saveFileList = new List<string>();
            try
            {
                var building = registryContext.Buildings.SingleOrDefault(b => b.IdBuilding == viewModel.IdBuilding);
                building.DemolishedPlanDate = viewModel.DemolishedPlanDate;
                building.DemolishedFactDate = viewModel.DemolishedFactDate;
                building.DateOwnerEmergency = viewModel.DateOwnerEmergency;
                building.DemandForDemolishingDeliveryDate = viewModel.DemandForDemolishingDeliveryDate;
                var oldBDActFiles = registryContext.BuildingDemolitionActFiles
                    .Include(af => af.ActFile)
                    .Where(af => af.IdBuilding == viewModel.IdBuilding)
                    .AsNoTracking();
                var newBDActFiles = viewModel.BuildingDemolitionActFiles ?? new List<BuildingDemolitionActFile>();
                var removeFileList = new List<string>();
                var i = 0; //итератор для массива файлов
                foreach (var newBDActFile in newBDActFiles)
                {
                    //Физический файл не был добавлен
                    if (newBDActFile.IdActFile == null) { }
                    //Физический файл добавили
                    else if (newBDActFile.IdActFile == 0)
                    {
                        //Добавляем новый физический файл
                        var nameNewActFile = reportService.SaveFormFileToRepository(viewModel.Files[i], ActFileTypes.BuildingDemolitionActFile);
                        saveFileList.Add(nameNewActFile);
                        //Проверяем был ли другой физический файл прикреплен ранее
                        foreach (var oldBDActFile in oldBDActFiles)
                        {
                            if (oldBDActFile.Id == newBDActFile.Id && oldBDActFile.ActFile != null)
                            {
                                //Удаляем старый физическимй файл
                                var nameOldActFile = oldBDActFile.ActFile.FileName;
                                removeFileList.Add(nameOldActFile);

                                //Переписываем запись о старом физфайлу данными о новом физфайле
                                oldBDActFile.ActFile.OriginalName = Path.GetFileName(viewModel.Files[i].FileName);
                                oldBDActFile.ActFile.FileName = nameNewActFile;
                                oldBDActFile.ActFile.MimeType = viewModel.Files[i].ContentType;

                                newBDActFile.ActFile = oldBDActFile.ActFile;
                            }
                        }
                        if (newBDActFile.ActFile == null)
                        {
                            newBDActFile.ActFile = new ActFile()
                            {
                                FileName = nameNewActFile,
                                OriginalName = Path.GetFileName(viewModel.Files[i].FileName),
                                MimeType = viewModel.Files[i].ContentType
                        };
                        }
                        i++;
                    }
                    //Физический файл удалили
                    else if (newBDActFile.IdActFile < 0)
                    {
                        //Находим какой файл был ранее прикреплен
                        newBDActFile.IdActFile = -1 * newBDActFile.IdActFile;
                        var oldActFile = registryContext.ActFiles.FirstOrDefault(af => af.IdFile == newBDActFile.IdActFile);
                        //Удаляем старый физический файл
                        registryContext.ActFiles.Remove(oldActFile);
                        //Если файл был удален явно, то удаляем
                        reportService.DeleteFileToRepository(oldActFile.FileName, ActFileTypes.BuildingDemolitionActFile);
                        removeFileList.Add(oldActFile.FileName);
                    }
                }
                foreach (var oldBDActFile in oldBDActFiles)
                {
                    //Человек удалил информацию о сносе
                    if (newBDActFiles.Select(af => af.Id).Contains(oldBDActFile.Id) == false)
                    {
                        //Информацию о сносе помечаем на удаление
                        registryContext.Entry(oldBDActFile).Property(p => p.Deleted).IsModified = true;
                        oldBDActFile.Deleted = 1;
                        newBDActFiles.Add(oldBDActFile);

                        //Если к записи был прикреплен физический файл
                        if (oldBDActFile.ActFile != null)
                        {
                            //Удаляем запись про физический файл
                            registryContext.ActFiles.Remove(oldBDActFile.ActFile);

                            //Удаляем сам физический файл
                            removeFileList.Add(oldBDActFile.ActFile.FileName);
                            
                        }
                    }
                }
                registryContext.BuildingDemolitionActFiles.UpdateRange(newBDActFiles);
                registryContext.SaveChanges();
                //Старые файлы не удаляем, на случай ошибочного удаления
                //removeFileList.ForEach(f => reportService.DeleteFileToRepository(f, ActFileTypes.BuildingDemolitionActFile));
                return Json(newBDActFiles.Select(f => f.IdActFile));
            }
            catch(Exception ex)
            {
                saveFileList.ForEach(f => reportService.DeleteFileToRepository(f, ActFileTypes.BuildingDemolitionActFile));
                return Json(-3);
            }
        }
    }
}